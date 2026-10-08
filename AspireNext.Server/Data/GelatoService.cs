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

    private static (decimal, decimal)? ParseSizeCm(string title)
    {
        var match = SizeCmRegex().Match(title);
        if (!match.Success)
            return null;

        return (decimal.Parse(match.Groups[1].Value), decimal.Parse(match.Groups[2].Value));
    }

    [GeneratedRegex(@"(\d+(?:\.\d+)?)\s*x\s*(\d+(?:\.\d+)?)\s*cm", RegexOptions.IgnoreCase)]
    private static partial Regex SizeCmRegex();

    public static void ConfigureAuthHeader(HttpClient client, GelatoOptions options) =>
        client.DefaultRequestHeaders.TryAddWithoutValidation("X-API-KEY", options.ApiKey);
}
