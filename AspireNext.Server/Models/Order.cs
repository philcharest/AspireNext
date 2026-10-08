namespace AspireNext.Server.Models;

public enum OrderStatus
{
    PendingPayment,
    Paid,
    PaymentFailed,
    Cancelled,
}

/// <summary>
/// Tracks the order's progress through Gelato print fulfillment, independently of
/// <see cref="OrderStatus"/> (which only tracks payment).
/// </summary>
public enum FulfillmentStatus
{
    NotSubmitted,
    Submitted,
    InProduction,
    Shipped,
    Delivered,
    Failed,
}

public class Order
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.PendingPayment;
    public string Currency { get; set; } = "CAD";
    public string? StripeCheckoutSessionId { get; set; }
    public string? StripePaymentIntentId { get; set; }
    public ShippingAddress? ShippingAddress { get; set; }
    public FulfillmentStatus FulfillmentStatus { get; set; } = FulfillmentStatus.NotSubmitted;
    public string? GelatoOrderId { get; set; }
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
