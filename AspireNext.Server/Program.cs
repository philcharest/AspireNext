using AspireNext.Server;
using AspireNext.Server.Data;
using AspireNext.Server.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
builder.AddRedisClientBuilder("cache")
    .WithOutputCache()
    .WithDistributedCache();
builder.AddNpgsqlDbContext<CatalogDbContext>("catalogdb");

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddScoped<CartService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    db.Database.Migrate();
    await CatalogSeeder.SeedAsync(db);
}

app.UseOutputCache();

string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

var api = app.MapGroup("/api");
api.MapGet("weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.CacheOutput(p => p.Expire(TimeSpan.FromSeconds(5)))
.WithName("GetWeatherForecast");

api.MapGet("products", async (CatalogDbContext db) =>
    await db.Products.Select(ProductDto.Projection).ToListAsync())
    .WithName("GetProducts");

api.MapGet("products/{id:int}", async (int id, CatalogDbContext db) =>
    await db.Products.Where(p => p.Id == id).Select(ProductDto.Projection).FirstOrDefaultAsync()
        is ProductDto product
        ? Results.Ok(product)
        : Results.NotFound())
    .WithName("GetProductById");

var cart = api.MapGroup("/cart");

cart.MapGet("", (HttpContext context, CartService cartService) =>
    cartService.GetCartAsync(CartCookie.GetOrCreateCartId(context)))
    .WithName("GetCart");

cart.MapPost("items", async (HttpContext context, AddCartItemRequest request, CartService cartService) =>
{
    try
    {
        return Results.Ok(await cartService.AddItemAsync(CartCookie.GetOrCreateCartId(context), request.ProductId, request.Quantity));
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound();
    }
    catch (ArgumentOutOfRangeException ex)
    {
        return Results.BadRequest(ex.Message);
    }
})
    .WithName("AddCartItem");

cart.MapPut("items/{productId:int}", async (int productId, HttpContext context, UpdateCartItemRequest request, CartService cartService) =>
    Results.Ok(await cartService.UpdateItemAsync(CartCookie.GetOrCreateCartId(context), productId, request.Quantity)))
    .WithName("UpdateCartItem");

cart.MapDelete("items/{productId:int}", async (int productId, HttpContext context, CartService cartService) =>
    Results.Ok(await cartService.RemoveItemAsync(CartCookie.GetOrCreateCartId(context), productId)))
    .WithName("RemoveCartItem");

app.MapDefaultEndpoints();

app.UseFileServer();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
