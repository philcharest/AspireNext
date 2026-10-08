using AspireNext.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AspireNext.Server.Data;

public class OrderService(AppDbContext db, CartService cartService, GelatoService gelatoService, IOptions<GelatoOptions> gelatoOptions, IConfiguration configuration)
{
    /// <summary>
    /// Snapshots the cart into a new order awaiting payment. The cart is intentionally left
    /// intact - it's only cleared once a webhook confirms the payment actually succeeded, so an
    /// abandoned or failed checkout doesn't lose the user's cart.
    /// </summary>
    public async Task<Order> CreatePendingOrderAsync(string userId, string cartId)
    {
        var cart = await cartService.GetCartAsync(cartId);
        if (cart.Items.Count == 0)
            throw new InvalidOperationException("Cart is empty.");

        var order = new Order
        {
            UserId = userId,
            CreatedAt = DateTimeOffset.UtcNow,
            Currency = cart.Currency,
            Items = [.. cart.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                ProductName = i.Name,
                Price = i.Price,
                Quantity = i.Quantity,
            })],
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        return order;
    }

    public async Task SetStripeSessionIdAsync(int orderId, string sessionId)
    {
        var order = await db.Orders.FindAsync(orderId) ?? throw new KeyNotFoundException($"Order {orderId} not found.");
        order.StripeCheckoutSessionId = sessionId;
        await db.SaveChangesAsync();
    }

    public Task<Order?> GetOrderByStripeSessionIdAsync(string sessionId) =>
        db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.StripeCheckoutSessionId == sessionId);

    public async Task MarkOrderPaidAsync(Order order, string paymentIntentId, ShippingAddress? shippingAddress)
    {
        order.Status = OrderStatus.Paid;
        order.StripePaymentIntentId = paymentIntentId;
        order.ShippingAddress = shippingAddress;
        await db.SaveChangesAsync();
        await cartService.ClearCartAsync($"user:{order.UserId}");

        await SubmitToGelatoAsync(order);
    }

    public async Task MarkOrderFailedAsync(Order order)
    {
        order.Status = OrderStatus.PaymentFailed;
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// (Re)submits a paid order to Gelato for print fulfillment. Called automatically once a
    /// Stripe payment is confirmed (see MarkOrderPaidAsync); also exposed to admins
    /// (AdminOrdersController) to retry after fixing whatever caused FulfillmentStatus.Failed -
    /// e.g. a product missing its Product.GelatoProductUid mapping, expected until your Gelato
    /// account/catalog is configured.
    /// </summary>
    public async Task<Order> ResubmitFulfillmentAsync(int orderId)
    {
        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new KeyNotFoundException($"Order {orderId} not found.");

        if (order.ShippingAddress is null)
            throw new InvalidOperationException("Order has no shipping address on file.");

        await SubmitToGelatoAsync(order);
        return order;
    }

    public async Task UpdateFulfillmentStatusAsync(string gelatoOrderId, FulfillmentStatus status)
    {
        var order = await db.Orders.FirstOrDefaultAsync(o => o.GelatoOrderId == gelatoOrderId)
            ?? throw new KeyNotFoundException($"No order found for Gelato order {gelatoOrderId}.");

        order.FulfillmentStatus = status;
        await db.SaveChangesAsync();
    }

    private async Task SubmitToGelatoAsync(Order order)
    {
        if (order.ShippingAddress is null)
        {
            // No shipping address was collected at checkout (e.g. Stripe's shipping address
            // collection isn't enabled yet) - nothing to submit until there is one.
            order.FulfillmentStatus = FulfillmentStatus.Failed;
            await db.SaveChangesAsync();
            return;
        }

        var productIds = order.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await db.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);

        if (order.Items.Any(i => string.IsNullOrEmpty(products.GetValueOrDefault(i.ProductId)?.GelatoProductUid)))
        {
            // At least one line item's product isn't mapped to a Gelato productUid yet (see
            // Product.GelatoProductUid) - leave the order flagged so an admin can retry once it is.
            order.FulfillmentStatus = FulfillmentStatus.Failed;
            await db.SaveChangesAsync();
            return;
        }

        foreach (var product in products.Values)
        {
            if (product.CanvasWidthCm is not { } widthCm || product.CanvasHeightCm is not { } heightCm)
                continue; // no physical size recorded for this product yet - nothing to validate

            if (!await gelatoService.IsSupportedCanvasSizeAsync(widthCm, heightCm))
            {
                // The product's recorded canvas size doesn't match anything in Gelato's live
                // catalog - don't submit an order Gelato would just reject; flag it so an admin
                // can fix the product's CanvasWidthCm/CanvasHeightCm before retrying.
                order.FulfillmentStatus = FulfillmentStatus.Failed;
                await db.SaveChangesAsync();
                return;
            }
        }

        // Gelato needs a publicly reachable HTTPS URL to download each print file from - this only
        // resolves to something Gelato can actually reach once the frontend is deployed publicly,
        // not against a local dev frontend.
        var frontendBaseUrl =
            configuration["services:frontend:https:0"] ??
            configuration["services:frontend:http:0"] ??
            throw new InvalidOperationException("Frontend base URL is not configured.");

        var address = order.ShippingAddress;
        var request = new GelatoOrderRequest(
            OrderType: "order",
            OrderReferenceId: order.Id.ToString(),
            CustomerReferenceId: order.UserId,
            Currency: order.Currency,
            Items: [.. order.Items.Select(i => new GelatoOrderItemRequest(
                ItemReferenceId: i.Id.ToString(),
                ProductUid: products[i.ProductId].GelatoProductUid!,
                Files: [new GelatoFile("default", $"{frontendBaseUrl}{products[i.ProductId].ImageUrl}")],
                Quantity: i.Quantity))],
            ShippingAddress: new GelatoAddress(
                FirstName: address.FirstName,
                LastName: address.LastName,
                AddressLine1: address.AddressLine1,
                AddressLine2: address.AddressLine2,
                City: address.City,
                State: address.State,
                PostCode: address.PostCode,
                Country: address.Country,
                Email: address.Email,
                Phone: address.Phone),
            ShipmentMethodUid: gelatoOptions.Value.DefaultShipmentMethodUid);

        try
        {
            var response = await gelatoService.CreateOrderAsync(request);
            order.GelatoOrderId = response.Id;
            order.FulfillmentStatus = FulfillmentStatus.Submitted;
        }
        catch (HttpRequestException)
        {
            order.FulfillmentStatus = FulfillmentStatus.Failed;
        }

        await db.SaveChangesAsync();
    }

    public async Task<List<OrderDto>> GetOrdersAsync(string userId)
    {
        var orders = await db.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var returnsByOrderId = await LoadReturnsByOrderIdAsync([.. orders.Select(o => o.Id)]);
        return [.. orders.Select(o => ToDto(o, returnsByOrderId.GetValueOrDefault(o.Id, [])))];
    }

    public async Task<OrderDto?> GetOrderAsync(string userId, int orderId)
    {
        var order = await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order is null)
            return null;

        var returnsByOrderId = await LoadReturnsByOrderIdAsync([order.Id]);
        return ToDto(order, returnsByOrderId.GetValueOrDefault(order.Id, []));
    }

    public async Task<List<AdminOrderDto>> GetAllOrdersAsync()
    {
        var orders = await db.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var userIds = orders.Select(o => o.UserId).Distinct().ToList();
        var emails = await db.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Email ?? "");

        return [.. orders.Select(o => new AdminOrderDto(
            o.Id,
            o.CreatedAt,
            o.Status,
            o.Currency,
            o.FulfillmentStatus,
            o.GelatoOrderId,
            o.Items.Sum(i => i.Price * i.Quantity),
            emails.GetValueOrDefault(o.UserId, "")))];
    }

    private async Task<Dictionary<int, List<Return>>> LoadReturnsByOrderIdAsync(List<int> orderIds)
    {
        var returns = await db.Returns
            .Where(r => orderIds.Contains(r.OrderId))
            .Include(r => r.Items)
            .ToListAsync();

        return returns.GroupBy(r => r.OrderId).ToDictionary(g => g.Key, g => g.ToList());
    }

    private static OrderDto ToDto(Order order, List<Return> returns) => new(
        order.Id,
        order.CreatedAt,
        order.Status,
        order.Currency,
        order.FulfillmentStatus,
        [.. order.Items.Select(i => new OrderItemDto(i.Id, i.ProductId, i.ProductName, i.Price, i.Quantity))],
        [.. returns.Select(r => ReturnDto.FromEntity(r, order.Items))]);
}
