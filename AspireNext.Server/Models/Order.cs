namespace AspireNext.Server.Models;

public enum OrderStatus
{
    Placed,
}

public class Order
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Placed;
    public List<OrderItem> Items { get; set; } = [];
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public required string ProductName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
