namespace AspireNext.Server.Models;

public record CartLine(int ProductId, int Quantity);

public record CartItemDto(int ProductId, string Name, string? ImageUrl, decimal Price, int Quantity)
{
    public decimal LineTotal => Price * Quantity;
}

public record CartDto(List<CartItemDto> Items)
{
    public decimal Total => Items.Sum(i => i.LineTotal);
}

public record AddCartItemRequest(int ProductId, int Quantity);

public record UpdateCartItemRequest(int Quantity);
