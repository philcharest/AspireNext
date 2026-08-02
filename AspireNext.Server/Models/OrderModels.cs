namespace AspireNext.Server.Models;

public record OrderItemDto(int ProductId, string ProductName, decimal Price, int Quantity)
{
    public decimal LineTotal => Price * Quantity;
}

public record OrderDto(int Id, DateTimeOffset CreatedAt, OrderStatus Status, List<OrderItemDto> Items)
{
    public decimal Total => Items.Sum(i => i.LineTotal);
}
