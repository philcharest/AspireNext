namespace AspireNext.Server.Models;

public enum CanvasOrientation
{
    Portrait,
    Landscape,
}

/// <summary>
/// A sellable canvas size, shared by every product of the matching orientation. Dimensions are
/// stored portrait-first (width &lt;= height); landscape products sell the same size rotated.
/// Prices are per size rather than per product - see docs/gelato-canvas-catalog.md for the Gelato
/// costs they were derived from.
/// </summary>
public class CanvasSize
{
    public int Id { get; set; }
    public decimal WidthCm { get; set; }
    public decimal HeightCm { get; set; }

    // Gelato's CanvasFormat attribute value for this size, e.g. "400x600-mm" - combined with the
    // frame and orientation by GelatoService.BuildCanvasProductUid to get the order's productUid.
    public required string GelatoFormat { get; set; }

    public decimal Price { get; set; }
    public decimal PriceUsd { get; set; }
    public int SortOrder { get; set; }

    public string LabelFor(CanvasOrientation orientation) =>
        orientation == CanvasOrientation.Landscape
            ? $"{HeightCm:0.#} × {WidthCm:0.#} cm"
            : $"{WidthCm:0.#} × {HeightCm:0.#} cm";

    public decimal PriceIn(string currency) => currency == "USD" ? PriceUsd : Price;
}
