namespace AspireNext.Server.Models;

public enum OrderStatus
{
    PendingPayment,
    Paid,
    PaymentFailed,
    Cancelled,
}

public class Order
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.PendingPayment;
    public string? StripeCheckoutSessionId { get; set; }
    public string? StripePaymentIntentId { get; set; }
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
