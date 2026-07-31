using System.Linq.Expressions;

namespace AspireNext.Server.Models;

public record ProductDto(int Id, string Name, string? Description, string? ImageUrl, decimal Price, string? CategoryName)
{
    public static Expression<Func<Product, ProductDto>> Projection { get; } = p =>
        new ProductDto(p.Id, p.Name, p.Description, p.ImageUrl, p.Price, p.Category != null ? p.Category.Name : null);
}
