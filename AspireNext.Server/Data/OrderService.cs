using AspireNext.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace AspireNext.Server.Data;

public class OrderService(AppDbContext db, CartService cartService)
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

    public async Task MarkOrderPaidAsync(Order order, string paymentIntentId)
    {
        order.Status = OrderStatus.Paid;
        order.StripePaymentIntentId = paymentIntentId;
        await db.SaveChangesAsync();
        await cartService.ClearCartAsync($"user:{order.UserId}");
    }

    public async Task MarkOrderFailedAsync(Order order)
    {
        order.Status = OrderStatus.PaymentFailed;
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
        [.. order.Items.Select(i => new OrderItemDto(i.Id, i.ProductId, i.ProductName, i.Price, i.Quantity))],
        [.. returns.Select(r => ReturnDto.FromEntity(r, order.Items))]);
}
