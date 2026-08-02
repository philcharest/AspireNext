using System.Security.Claims;
using System.Text.Json.Serialization;
using AspireNext.Server;
using AspireNext.Server.Data;
using AspireNext.Server.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
builder.AddRedisClientBuilder("cache")
    .WithOutputCache()
    .WithDistributedCache();
builder.AddNpgsqlDbContext<AppDbContext>("catalogdb");

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
    .AddEntityFrameworkStores<AppDbContext>();
builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    await CatalogSeeder.SeedAsync(db);
}

app.UseAuthentication();
app.UseAuthorization();

app.UseOutputCache();

string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

var api = app.MapGroup("/api");

api.MapGet("antiforgery/token", (HttpContext context, IAntiforgery antiforgery) =>
{
    var tokens = antiforgery.GetAndStoreTokens(context);
    return Results.Ok(new { token = tokens.RequestToken });
})
    .WithName("GetAntiforgeryToken");

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

api.MapGet("products", async (AppDbContext db) =>
    await db.Products.Select(ProductDto.Projection).ToListAsync())
    .WithName("GetProducts");

api.MapGet("products/{id:int}", async (int id, AppDbContext db) =>
    await db.Products.Where(p => p.Id == id).Select(ProductDto.Projection).FirstOrDefaultAsync()
        is ProductDto product
        ? Results.Ok(product)
        : Results.NotFound())
    .WithName("GetProductById");

api.MapIdentityApi<ApplicationUser>();

api.MapGet("account/me", (ClaimsPrincipal user) =>
    user.Identity?.IsAuthenticated == true
        ? Results.Ok(new { email = user.FindFirstValue(ClaimTypes.Email) })
        : Results.Unauthorized())
    .WithName("GetCurrentUser");

api.MapPost("account/logout", async (SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Ok();
})
    .RequireAuthorization()
    .AddEndpointFilter(AntiforgeryFilter.ValidateAsync)
    .WithName("Logout");

var cart = api.MapGroup("/cart");
cart.AddEndpointFilter(AntiforgeryFilter.ValidateAsync);

cart.MapGet("", (HttpContext context, CartService cartService) =>
    cartService.GetCartAsync(CartCookie.ResolveCartKey(context)))
    .WithName("GetCart");

cart.MapPost("items", async (HttpContext context, AddCartItemRequest request, CartService cartService) =>
{
    try
    {
        return Results.Ok(await cartService.AddItemAsync(CartCookie.ResolveCartKey(context), request.ProductId, request.Quantity));
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
    Results.Ok(await cartService.UpdateItemAsync(CartCookie.ResolveCartKey(context), productId, request.Quantity)))
    .WithName("UpdateCartItem");

cart.MapDelete("items/{productId:int}", async (int productId, HttpContext context, CartService cartService) =>
    Results.Ok(await cartService.RemoveItemAsync(CartCookie.ResolveCartKey(context), productId)))
    .WithName("RemoveCartItem");

cart.MapPost("merge", async (HttpContext context, CartService cartService) =>
    Results.Ok(await cartService.MergeCartsAsync(CartCookie.GetAnonymousCartKey(context), CartCookie.ResolveCartKey(context))))
    .RequireAuthorization()
    .WithName("MergeCart");

api.MapPost("checkout", async (HttpContext context, ClaimsPrincipal user, OrderService orderService) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
    try
    {
        return Results.Ok(await orderService.CheckoutAsync(userId, CartCookie.ResolveCartKey(context)));
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(ex.Message);
    }
})
    .RequireAuthorization()
    .AddEndpointFilter(AntiforgeryFilter.ValidateAsync)
    .WithName("Checkout");

api.MapGet("orders", (ClaimsPrincipal user, OrderService orderService) =>
    orderService.GetOrdersAsync(user.FindFirstValue(ClaimTypes.NameIdentifier)!))
    .RequireAuthorization()
    .WithName("GetOrders");

api.MapGet("orders/{id:int}", async (int id, ClaimsPrincipal user, OrderService orderService) =>
    await orderService.GetOrderAsync(user.FindFirstValue(ClaimTypes.NameIdentifier)!, id)
        is OrderDto order
        ? Results.Ok(order)
        : Results.NotFound())
    .RequireAuthorization()
    .WithName("GetOrderById");

app.MapDefaultEndpoints();

app.UseFileServer();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
