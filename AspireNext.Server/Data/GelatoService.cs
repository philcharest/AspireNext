using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using AspireNext.Server.Models;
using Microsoft.Extensions.Options;

namespace AspireNext.Server.Data;

/// <summary>
/// Thin client for Gelato's Order API (v4) and Product Catalog API (v3). Registered as a typed
/// HttpClient (see Program.cs), which picks up this app's standard resilience handler
/// (retry/circuit-breaker/timeout) automatically via ConfigureHttpClientDefaults.
/// </summary>
public partial class GelatoService(HttpClient httpClient, IOptions<GelatoOptions> options)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly GelatoOptions _options = options.Value;

    public async Task<GelatoOrderResponse> CreateOrderAsync(GelatoOrderRequest request)
    {
        var response = await httpClient.PostAsJsonAsync($"{_options.OrderApiBaseUrl}/orders", request, JsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<GelatoOrderResponse>(JsonOptions))!;
    }

    public async Task<GelatoOrderResponse?> GetOrderAsync(string gelatoOrderId)
    {
        var response = await httpClient.GetAsync($"{_options.OrderApiBaseUrl}/orders/{gelatoOrderId}");
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GelatoOrderResponse>(JsonOptions);
    }

    /// <summary>
    /// Fetches the canvas sizes Gelato's live catalog currently offers, in centimetres, by reading
    /// the size attribute's values off the catalog named by GelatoOptions.CanvasCatalogUid. Those
    /// option names are placeholders - confirm them against your account's real catalog
    /// (GET {ProductApiBaseUrl}/catalogs lists every catalogUid and its attributes) before relying
    /// on this for validation.
    /// </summary>
    public async Task<List<(decimal WidthCm, decimal HeightCm)>> GetSupportedCanvasSizesAsync()
    {
        var response = await httpClient.GetAsync($"{_options.ProductApiBaseUrl}/catalogs/{_options.CanvasCatalogUid}");
        response.EnsureSuccessStatusCode();

        var catalog = await response.Content.ReadFromJsonAsync<GelatoCatalogResponse>(JsonOptions);
        var sizeAttribute = catalog?.ProductAttributes?
            .FirstOrDefault(a => a.ProductAttributeUid == _options.CanvasSizeAttributeUid);

        if (sizeAttribute is null)
            return [];

        return [.. sizeAttribute.Values
            .Select(v => ParseSizeCm(v.Title))
            .OfType<(decimal, decimal)>()];
    }

    /// <summary>
    /// Checks a canvas's stored dimensions against Gelato's live catalog, orientation-agnostic
    /// (a 40x50 product matches a catalog size listed either as 40x50 or 50x40) with a small
    /// tolerance for rounding. Fails open (returns true) if the catalog can't be read at all, so a
    /// transient Gelato outage doesn't block checkout - this only catches genuinely unsupported sizes.
    /// </summary>
    public async Task<bool> IsSupportedCanvasSizeAsync(decimal widthCm, decimal heightCm, decimal toleranceCm = 0.5m)
    {
        List<(decimal WidthCm, decimal HeightCm)> sizes;
        try
        {
            sizes = await GetSupportedCanvasSizesAsync();
        }
        catch (HttpRequestException)
        {
            return true;
        }

        return sizes.Count == 0 || sizes.Any(s =>
            (Math.Abs(s.WidthCm - widthCm) <= toleranceCm && Math.Abs(s.HeightCm - heightCm) <= toleranceCm) ||
            (Math.Abs(s.WidthCm - heightCm) <= toleranceCm && Math.Abs(s.HeightCm - widthCm) <= toleranceCm));
    }

    // Gelato titles its canvas sizes as e.g. "500x700 mm" or "24x30 inches" - normalize both to cm.
    private static (decimal, decimal)? ParseSizeCm(string title)
    {
        var match = SizeRegex().Match(title);
        if (!match.Success)
            return null;

        var toCm = match.Groups[3].Value.ToLowerInvariant() switch
        {
            "mm" => 0.1m,
            "cm" => 1m,
            _ => 2.54m, // in / inch / inches
        };
        return (decimal.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) * toCm,
                decimal.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture) * toCm);
    }

    [GeneratedRegex(@"(\d+(?:\.\d+)?)\s*x\s*(\d+(?:\.\d+)?)\s*(mm|cm|inches|inch|in)\b", RegexOptions.IgnoreCase)]
    private static partial Regex SizeRegex();

    // e.g. canvas_product_cf_400x600-mm_cm_canvas_cfrm_wood-fsc-2-cm_cl_4-0_ver - the pattern of
    // Gelato's frame-depth canvas products (see docs/gelato-canvas-catalog.md).
    public string BuildCanvasProductUid(CanvasSize size, CanvasOrientation orientation) =>
        $"canvas_product_cf_{size.GelatoFormat}_cm_canvas_cfrm_{_options.CanvasFrameUid}_cl_4-0_{(orientation == CanvasOrientation.Landscape ? "hor" : "ver")}";

    public static void ConfigureAuthHeader(HttpClient client, GelatoOptions options) =>
        client.DefaultRequestHeaders.TryAddWithoutValidation("X-API-KEY", options.ApiKey);
}
