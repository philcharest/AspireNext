namespace AspireNext.Server.Models;

public enum ReturnStatus
{
    Requested,
    Approved,
    Rejected,
}

public class Return
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public DateTimeOffset RequestedAt { get; set; }
    public ReturnStatus Status { get; set; } = ReturnStatus.Requested;
    public required string Reason { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public string? ReviewNote { get; set; }
    public string? StripeRefundId { get; set; }
    public decimal? RefundAmount { get; set; }
    public List<ReturnItem> Items { get; set; } = [];
}

public class ReturnItem
{
    public int Id { get; set; }
    public int ReturnId { get; set; }
    public int OrderItemId { get; set; }
    public int Quantity { get; set; }
}
