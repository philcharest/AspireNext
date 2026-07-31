using System.Text.Json;
using AspireNext.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace AspireNext.Server.Data;

public class CartService(IDistributedCache cache, CatalogDbContext db)
{
    private static readonly TimeSpan CartLifetime = TimeSpan.FromDays(30);

    public async Task<CartDto> GetCartAsync(string cartId)
    {
        var lines = await LoadLinesAsync(cartId);
        return await ToCartDtoAsync(lines);
    }

    public async Task<CartDto> AddItemAsync(string cartId, int productId, int quantity)
    {
        if (quantity < 1)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be at least 1.");

        if (!await db.Products.AnyAsync(p => p.Id == productId))
            throw new KeyNotFoundException($"Product {productId} not found.");

        var lines = await LoadLinesAsync(cartId);
        var existing = lines.FirstOrDefault(l => l.ProductId == productId);
        lines = existing is null
            ? [.. lines, new CartLine(productId, quantity)]
            : [.. lines.Where(l => l.ProductId != productId), existing with { Quantity = existing.Quantity + quantity }];

        await SaveLinesAsync(cartId, lines);
        return await ToCartDtoAsync(lines);
    }

    public async Task<CartDto> UpdateItemAsync(string cartId, int productId, int quantity)
    {
        var lines = await LoadLinesAsync(cartId);
        lines = quantity < 1
            ? [.. lines.Where(l => l.ProductId != productId)]
            : [.. lines.Select(l => l.ProductId == productId ? l with { Quantity = quantity } : l)];

        await SaveLinesAsync(cartId, lines);
        return await ToCartDtoAsync(lines);
    }

    public async Task<CartDto> RemoveItemAsync(string cartId, int productId)
    {
        var lines = await LoadLinesAsync(cartId);
        lines = [.. lines.Where(l => l.ProductId != productId)];
        await SaveLinesAsync(cartId, lines);
        return await ToCartDtoAsync(lines);
    }

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

    private async Task<CartDto> ToCartDtoAsync(List<CartLine> lines)
    {
        if (lines.Count == 0)
            return new CartDto([]);

        var productIds = lines.Select(l => l.ProductId).ToList();
        var products = await db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        var items = lines
            .Where(l => products.ContainsKey(l.ProductId))
            .Select(l =>
            {
                var product = products[l.ProductId];
                return new CartItemDto(product.Id, product.Name, product.ImageUrl, product.Price, l.Quantity);
            })
            .ToList();

        return new CartDto(items);
    }

    private static string CacheKey(string cartId) => $"cart:{cartId}";
}
