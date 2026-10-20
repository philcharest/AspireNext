namespace AspireNext.Server.Models;

// A cart line is one product in one canvas size - the same artwork in two sizes is two lines.
public record CartLine(int ProductId, int SizeId, int Quantity);

public record CartItemDto(int ProductId, int SizeId, string Name, string SizeLabel, string? ImageUrl, decimal Price, int Quantity)
{
    public decimal LineTotal => Price * Quantity;
}

public record CartDto(List<CartItemDto> Items, string Currency)
{
    public decimal Total => Items.Sum(i => i.LineTotal);
}

public record AddCartItemRequest(int ProductId, int SizeId, int Quantity);

public record UpdateCartItemRequest(int Quantity);

public record UpdateCartCurrencyRequest(string Currency);
