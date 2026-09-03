using System.Text.Json.Serialization;
using AspireNext.Server.Data;
using AspireNext.Server.Models;
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
builder.Services.AddScoped<ReturnService>();
builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection("Stripe"));
builder.Services.AddSingleton<StripeService>();
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddTransient<IEmailSender<ApplicationUser>, SmtpEmailSender>();
builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();
builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Controllers get their own JSON options separate from the minimal-API ones above (still needed
// for MapIdentityApi's responses) - keep enum-as-string serialization consistent across both.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

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

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await AdminSeeder.SeedAsync(userManager, roleManager);
}

app.UseAuthentication();
app.UseAuthorization();

app.UseOutputCache();

// MapIdentityApi<ApplicationUser>() has no official Controller equivalent - it's Microsoft's own
// built-in register/login/forgotPassword/etc. endpoint group, kept as the one minimal-API route
// registration in this file rather than hand-reimplementing Identity's own logic.
app.MapGroup("/api").MapIdentityApi<ApplicationUser>();

app.MapControllers();

app.MapDefaultEndpoints();

app.UseFileServer();

app.Run();
