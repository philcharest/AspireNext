namespace AspireNext.Server.Models;

public record ProductSizeDto(int Id, string Label, decimal WidthCm, decimal HeightCm, decimal Price, decimal PriceUsd);

// Price/PriceUsd are the cheapest size's prices, for "From $X" displays in listings.
public record ProductDto(int Id, string Name, string? Description, string? ImageUrl, decimal Price, decimal PriceUsd, string? CategoryName, CanvasOrientation Orientation, List<ProductSizeDto> Sizes)
{
    public static ProductDto From(Product product, List<CanvasSize> sizes)
    {
        var landscape = product.Orientation == CanvasOrientation.Landscape;
        List<ProductSizeDto> sizeDtos = [.. sizes.Select(s => new ProductSizeDto(
            s.Id,
            s.LabelFor(product.Orientation),
            landscape ? s.HeightCm : s.WidthCm,
            landscape ? s.WidthCm : s.HeightCm,
            s.Price,
            s.PriceUsd))];

        return new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.ImageUrl,
            sizeDtos.Count > 0 ? sizeDtos.Min(s => s.Price) : 0,
            sizeDtos.Count > 0 ? sizeDtos.Min(s => s.PriceUsd) : 0,
            product.Category?.Name,
            product.Orientation,
            sizeDtos);
    }
}
