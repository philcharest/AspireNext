namespace AspireNext.Server.Models;

public class Product
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? PriceUsd { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    // Canvas print dimensions, in centimetres - used to validate against Gelato's supported
    // canvas sizes (see GelatoService.IsSupportedCanvasSizeAsync) before an order can be
    // submitted for fulfillment.
    public decimal? CanvasWidthCm { get; set; }
    public decimal? CanvasHeightCm { get; set; }

    // PLACEHOLDER - the exact Gelato productUid for this product's canvas size/variant. Required
    // before this product's orders can be submitted to Gelato; see GelatoOptions and
    // https://dashboard.gelato.com/docs/products/ for how to look these up once your Gelato
    // account exists.
    public string? GelatoProductUid { get; set; }
}
