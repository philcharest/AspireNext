using AspireNext.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace AspireNext.Server.Data;

public class OrderService(AppDbContext db, CartService cartService)
{
    public async Task<OrderDto> CheckoutAsync(string userId, string cartId)
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
        await cartService.ClearCartAsync(cartId);

        return ToDto(order);
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
