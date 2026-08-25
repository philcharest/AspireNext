namespace AspireNext.Server.Models;

public record ReturnItemDto(int OrderItemId, string ProductName, int Quantity);

public record ReturnDto(
    int Id,
    DateTimeOffset RequestedAt,
    ReturnStatus Status,
    string Reason,
    string? ReviewNote,
    decimal? RefundAmount,
    List<ReturnItemDto> Items)
{
    public static ReturnDto FromEntity(Return ret, List<OrderItem> orderItems) => new(
        ret.Id,
        ret.RequestedAt,
        ret.Status,
        ret.Reason,
        ret.ReviewNote,
        ret.RefundAmount,
        [.. ret.Items.Select(ri => new ReturnItemDto(
            ri.OrderItemId,
            orderItems.First(oi => oi.Id == ri.OrderItemId).ProductName,
            ri.Quantity))]);
}

public record AdminReturnDto(
    int Id,
    int OrderId,
    string UserEmail,
    DateTimeOffset RequestedAt,
    ReturnStatus Status,
    string Reason,
    string? ReviewNote,
    decimal? RefundAmount,
    List<ReturnItemDto> Items);

public record CreateReturnItemRequest(int OrderItemId, int Quantity);

public record CreateReturnRequest(string Reason, List<CreateReturnItemRequest> Items);

public record RejectReturnRequest(string? Note);
