using AspireNext.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace AspireNext.Server.Data;

public class ReturnService(AppDbContext db, StripeService stripeService)
{
    public async Task<ReturnDto> CreateReturnRequestAsync(string userId, int orderId, CreateReturnRequest request)
    {
        var order = await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId)
            ?? throw new KeyNotFoundException($"Order {orderId} not found.");

        if (order.Status != OrderStatus.Paid)
            throw new InvalidOperationException("Only paid orders can be returned.");

        if (request.Items.Count == 0)
            throw new InvalidOperationException("Select at least one item to return.");

        // Requested and Approved returns both reserve quantity against future requests -
        // only Rejected ones free it back up.
        var existingReturns = await db.Returns
            .Where(r => r.OrderId == orderId && r.Status != ReturnStatus.Rejected)
            .Include(r => r.Items)
            .ToListAsync();

        var returnItems = new List<ReturnItem>();
        foreach (var itemRequest in request.Items)
        {
            var orderItem = order.Items.FirstOrDefault(i => i.Id == itemRequest.OrderItemId)
                ?? throw new InvalidOperationException($"Order item {itemRequest.OrderItemId} does not belong to this order.");

            if (itemRequest.Quantity < 1)
                throw new InvalidOperationException("Return quantity must be at least 1.");

            var alreadyRequested = existingReturns
                .SelectMany(r => r.Items)
                .Where(ri => ri.OrderItemId == orderItem.Id)
                .Sum(ri => ri.Quantity);

            var remaining = orderItem.Quantity - alreadyRequested;
            if (itemRequest.Quantity > remaining)
                throw new InvalidOperationException($"Cannot return more than the {remaining} remaining unit(s) of \"{orderItem.ProductName}\".");

            returnItems.Add(new ReturnItem { OrderItemId = orderItem.Id, Quantity = itemRequest.Quantity });
        }

        var newReturn = new Return
        {
            OrderId = orderId,
            RequestedAt = DateTimeOffset.UtcNow,
            Reason = request.Reason,
            Items = returnItems,
        };

        db.Returns.Add(newReturn);
        await db.SaveChangesAsync();

        return ReturnDto.FromEntity(newReturn, order.Items);
    }

    public async Task<List<AdminReturnDto>> GetAllReturnsAsync()
    {
        var returns = await db.Returns
            .Include(r => r.Items)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync();

        var orderIds = returns.Select(r => r.OrderId).Distinct().ToList();
        var orders = await db.Orders
            .Where(o => orderIds.Contains(o.Id))
            .Include(o => o.Items)
            .ToDictionaryAsync(o => o.Id);

        var userIds = orders.Values.Select(o => o.UserId).Distinct().ToList();
        var emails = await db.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Email ?? "");

        return [.. returns.Select(r =>
        {
            var order = orders[r.OrderId];
            var dto = ReturnDto.FromEntity(r, order.Items);
            return new AdminReturnDto(
                dto.Id,
                r.OrderId,
                emails.GetValueOrDefault(order.UserId, ""),
                dto.RequestedAt,
                dto.Status,
                dto.Reason,
                dto.ReviewNote,
                dto.RefundAmount,
                dto.Items);
        })];
    }

    public async Task<ReturnDto> ApproveReturnAsync(int returnId)
    {
        var (ret, order) = await LoadReturnWithOrderAsync(returnId);

        if (ret.Status != ReturnStatus.Requested)
            throw new InvalidOperationException("Only pending return requests can be approved.");

        if (order.StripePaymentIntentId is null)
            throw new InvalidOperationException("This order has no payment to refund.");

        var amount = ret.Items.Sum(ri => order.Items.First(oi => oi.Id == ri.OrderItemId).Price * ri.Quantity);
        var refund = await stripeService.RefundAsync(order.StripePaymentIntentId, amount);

        ret.Status = ReturnStatus.Approved;
        ret.ReviewedAt = DateTimeOffset.UtcNow;
        ret.StripeRefundId = refund.Id;
        ret.RefundAmount = amount;

        await db.SaveChangesAsync();

        return ReturnDto.FromEntity(ret, order.Items);
    }

    public async Task<ReturnDto> RejectReturnAsync(int returnId, string? note)
    {
        var (ret, order) = await LoadReturnWithOrderAsync(returnId);

        if (ret.Status != ReturnStatus.Requested)
            throw new InvalidOperationException("Only pending return requests can be rejected.");

        ret.Status = ReturnStatus.Rejected;
        ret.ReviewedAt = DateTimeOffset.UtcNow;
        ret.ReviewNote = note;

        await db.SaveChangesAsync();

        return ReturnDto.FromEntity(ret, order.Items);
    }

    private async Task<(Return Return, Order Order)> LoadReturnWithOrderAsync(int returnId)
    {
        var ret = await db.Returns
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == returnId)
            ?? throw new KeyNotFoundException($"Return {returnId} not found.");

        var order = await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == ret.OrderId)
            ?? throw new KeyNotFoundException($"Order {ret.OrderId} not found.");

        return (ret, order);
    }
}
