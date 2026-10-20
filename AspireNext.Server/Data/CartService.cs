using System.Text.Json;
using AspireNext.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace AspireNext.Server.Data;

public class CartService(IDistributedCache cache, AppDbContext db)
{
    private static readonly TimeSpan CartLifetime = TimeSpan.FromDays(30);
    private static readonly string[] SupportedCurrencies = ["CAD", "USD"];

    public async Task<CartDto> GetCartAsync(string cartId)
    {
        var lines = await LoadLinesAsync(cartId);
        var currency = await GetCurrencyAsync(cartId);
        return await ToCartDtoAsync(lines, currency);
    }

    public async Task<CartDto> AddItemAsync(string cartId, int productId, int sizeId, int quantity)
    {
        if (quantity < 1)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be at least 1.");

        if (!await db.Products.AnyAsync(p => p.Id == productId))
            throw new KeyNotFoundException($"Product {productId} not found.");

        if (!await db.CanvasSizes.AnyAsync(s => s.Id == sizeId))
            throw new KeyNotFoundException($"Canvas size {sizeId} not found.");

        var lines = await LoadLinesAsync(cartId);
        var existing = lines.FirstOrDefault(l => IsLine(l, productId, sizeId));
        lines = existing is null
            ? [.. lines, new CartLine(productId, sizeId, quantity)]
            : [.. lines.Where(l => !IsLine(l, productId, sizeId)), existing with { Quantity = existing.Quantity + quantity }];

        await SaveLinesAsync(cartId, lines);
        return await ToCartDtoAsync(lines, await GetCurrencyAsync(cartId));
    }

    public async Task<CartDto> UpdateItemAsync(string cartId, int productId, int sizeId, int quantity)
    {
        var lines = await LoadLinesAsync(cartId);
        lines = quantity < 1
            ? [.. lines.Where(l => !IsLine(l, productId, sizeId))]
            : [.. lines.Select(l => IsLine(l, productId, sizeId) ? l with { Quantity = quantity } : l)];

        await SaveLinesAsync(cartId, lines);
        return await ToCartDtoAsync(lines, await GetCurrencyAsync(cartId));
    }

    public async Task<CartDto> RemoveItemAsync(string cartId, int productId, int sizeId)
    {
        var lines = await LoadLinesAsync(cartId);
        lines = [.. lines.Where(l => !IsLine(l, productId, sizeId))];
        await SaveLinesAsync(cartId, lines);
        return await ToCartDtoAsync(lines, await GetCurrencyAsync(cartId));
    }

    public async Task<CartDto> SetCurrencyAsync(string cartId, string currency)
    {
        if (!SupportedCurrencies.Contains(currency))
            throw new ArgumentOutOfRangeException(nameof(currency), "Currency must be CAD or USD.");

        await cache.SetStringAsync(
            CurrencyCacheKey(cartId),
            currency,
            new DistributedCacheEntryOptions { SlidingExpiration = CartLifetime });

        var lines = await LoadLinesAsync(cartId);
        return await ToCartDtoAsync(lines, currency);
    }

    public Task ClearCartAsync(string cartId) =>
        Task.WhenAll(cache.RemoveAsync(CacheKey(cartId)), cache.RemoveAsync(CurrencyCacheKey(cartId)));

    /// <summary>
    /// Folds the cart at <paramref name="fromCartId"/> into the cart at <paramref name="intoCartId"/>,
    /// summing quantities for product/size lines present in both, then clears the source cart.
    /// </summary>
    public async Task<CartDto> MergeCartsAsync(string fromCartId, string intoCartId)
    {
        var fromLines = await LoadLinesAsync(fromCartId);
        if (fromLines.Count == 0)
            return await GetCartAsync(intoCartId);

        var intoLines = await LoadLinesAsync(intoCartId);
        var merged = new List<CartLine>(intoLines);
        foreach (var line in fromLines)
        {
            var index = merged.FindIndex(l => IsLine(l, line.ProductId, line.SizeId));
            if (index >= 0)
                merged[index] = merged[index] with { Quantity = merged[index].Quantity + line.Quantity };
            else
                merged.Add(line);
        }

        await SaveLinesAsync(intoCartId, merged);
        await ClearCartAsync(fromCartId);
        return await ToCartDtoAsync(merged, await GetCurrencyAsync(intoCartId));
    }

    private static bool IsLine(CartLine line, int productId, int sizeId) =>
        line.ProductId == productId && line.SizeId == sizeId;

    private async Task<List<CartLine>> LoadLinesAsync(string cartId)
    {
        var json = await cache.GetStringAsync(CacheKey(cartId));
        return json is null ? [] : JsonSerializer.Deserialize<List<CartLine>>(json) ?? [];
    }

    private Task SaveLinesAsync(string cartId, List<CartLine> lines) =>
        cache.SetStringAsync(
            CacheKey(cartId),
            JsonSerializer.Serialize(lines),
            new DistributedCacheEntryOptions { SlidingExpiration = CartLifetime });

    private async Task<string> GetCurrencyAsync(string cartId) =>
        await cache.GetStringAsync(CurrencyCacheKey(cartId)) ?? "CAD";

    private async Task<CartDto> ToCartDtoAsync(List<CartLine> lines, string currency)
    {
        if (lines.Count == 0)
            return new CartDto([], currency);

        var productIds = lines.Select(l => l.ProductId).ToList();
        var products = await db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        var sizeIds = lines.Select(l => l.SizeId).ToList();
        var sizes = await db.CanvasSizes
            .Where(s => sizeIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id);

        // Lines whose product or size no longer exists are skipped - this also drops lines saved
        // before carts tracked a size (they deserialize with SizeId 0).
        var items = lines
            .Where(l => products.ContainsKey(l.ProductId) && sizes.ContainsKey(l.SizeId))
            .Select(l =>
            {
                var product = products[l.ProductId];
                var size = sizes[l.SizeId];
                return new CartItemDto(
                    product.Id,
                    size.Id,
                    product.Name,
                    size.LabelFor(product.Orientation),
                    product.ImageUrl,
                    size.PriceIn(currency),
                    l.Quantity);
            })
            .ToList();

        return new CartDto(items, currency);
    }

    private static string CacheKey(string cartId) => $"cart:{cartId}";
    private static string CurrencyCacheKey(string cartId) => $"cart-currency:{cartId}";
}
