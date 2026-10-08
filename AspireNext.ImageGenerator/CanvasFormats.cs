using System;
using System.Collections.Generic;
using System.Linq;

namespace AspireNext.ImageGenerator;

public enum CanvasOrientation
{
    Portrait,
    Landscape,
    Square,
}

/// <summary>
/// A sellable canvas size paired with the SDXL native generation resolution whose aspect ratio
/// matches it most closely, chosen from SDXL's published ~1024x1024-area resolution buckets
/// (832x1216, 896x1152, 1024x1024 and their landscape mirrors) so compositions aren't stretched
/// or awkwardly cropped on the way to print. See Program.cs's ResolveSizing for how the refine/
/// upscale/crop stages scale up from NativeWidth/NativeHeight to the final print resolution.
/// </summary>
public record CanvasFormat(string Name, CanvasOrientation Orientation, decimal WidthCm, decimal HeightCm, int NativeWidth, int NativeHeight);

public static class CanvasFormats
{
    // PLACEHOLDER - these are the physical sizes this pipeline currently targets. Confirm they're
    // still exactly what your Gelato account's canvas catalog offers before relying on them -
    // AspireNext.Server's GelatoService.GetSupportedCanvasSizesAsync validates against the live
    // catalog (this list is intentionally kept in the same cm values so the two reconcile easily).
    public static readonly List<CanvasFormat> All =
    [
        new("Square 30x30",     CanvasOrientation.Square,    30, 30, 1024, 1024),
        new("Square 40x40",     CanvasOrientation.Square,    40, 40, 1024, 1024),
        new("Square 50x50",     CanvasOrientation.Square,    50, 50, 1024, 1024),
        new("Square 60x60",     CanvasOrientation.Square,    60, 60, 1024, 1024),

        new("Portrait 30x40",   CanvasOrientation.Portrait,  30,  40,  896, 1152),
        new("Landscape 40x30",  CanvasOrientation.Landscape, 40,  30, 1152,  896),
        new("Portrait 40x50",   CanvasOrientation.Portrait,  40,  50,  896, 1152),
        new("Landscape 50x40",  CanvasOrientation.Landscape, 50,  40, 1152,  896),
        new("Portrait 60x80",   CanvasOrientation.Portrait,  60,  80,  896, 1152),
        new("Landscape 80x60",  CanvasOrientation.Landscape, 80,  60, 1152,  896),

        new("Portrait 50x70",   CanvasOrientation.Portrait,  50,  70,  832, 1216),
        new("Landscape 70x50",  CanvasOrientation.Landscape, 70,  50, 1216,  832),
        new("Portrait 60x90",   CanvasOrientation.Portrait,  60,  90,  832, 1216),
        new("Landscape 90x60",  CanvasOrientation.Landscape, 90,  60, 1216,  832),
        new("Portrait 70x100",  CanvasOrientation.Portrait,  70, 100,  832, 1216),
        new("Landscape 100x70", CanvasOrientation.Landscape, 100, 70, 1216,  832),
    ];

    public static CanvasFormat PickRandom(CanvasOrientation orientation)
    {
        var candidates = All.Where(f => f.Orientation == orientation).ToList();
        return candidates[Random.Shared.Next(candidates.Count)];
    }
}
