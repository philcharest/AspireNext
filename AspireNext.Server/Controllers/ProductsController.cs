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
    public async Task<IActionResult> GetProducts() =>
        Ok(await db.Products.Select(ProductDto.Projection).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetProductById(int id) =>
        await db.Products.Where(p => p.Id == id).Select(ProductDto.Projection).FirstOrDefaultAsync()
            is ProductDto product
            ? Ok(product)
            : NotFound();
}
