namespace AspireNext.Server.Models;

public record OrderItemDto(int Id, int ProductId, string ProductName, decimal Price, int Quantity)
{
    public decimal LineTotal => Price * Quantity;
}

public record OrderDto(int Id, DateTimeOffset CreatedAt, OrderStatus Status, string Currency, FulfillmentStatus FulfillmentStatus, List<OrderItemDto> Items, List<ReturnDto> Returns)
{
    public decimal Total => Items.Sum(i => i.LineTotal);
}

public record AdminOrderDto(int Id, DateTimeOffset CreatedAt, OrderStatus Status, string Currency, FulfillmentStatus FulfillmentStatus, string? GelatoOrderId, decimal Total, string UserEmail);
