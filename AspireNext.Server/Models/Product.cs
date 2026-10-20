namespace AspireNext.Server.Models;

public class Product
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    // Decides which way round each CanvasSize is printed and which Gelato productUid ("ver"/"hor")
    // orders use. Pricing lives on CanvasSize - every product is sold in all sizes.
    public CanvasOrientation Orientation { get; set; } = CanvasOrientation.Portrait;
}
