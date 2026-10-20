using AspireNext.Server.Data;
using AspireNext.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspireNext.Server.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var sizes = await LoadSizesAsync();
        var products = await db.Products.Include(p => p.Category).ToListAsync();
        return Ok(products.Select(p => ProductDto.From(p, sizes)));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetProductById(int id)
    {
        var product = await db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        return product is null ? NotFound() : Ok(ProductDto.From(product, await LoadSizesAsync()));
    }

    private Task<List<CanvasSize>> LoadSizesAsync() =>
        db.CanvasSizes.OrderBy(s => s.SortOrder).ToListAsync();
}
