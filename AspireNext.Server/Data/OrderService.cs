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

        return [.. orders.Select(ToDto)];
    }

    public async Task<OrderDto?> GetOrderAsync(string userId, int orderId)
    {
        var order = await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        return order is null ? null : ToDto(order);
    }

    private static OrderDto ToDto(Order order) => new(
        order.Id,
        order.CreatedAt,
        order.Status,
        [.. order.Items.Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.Price, i.Quantity))]);
}
