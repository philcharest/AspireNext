using AspireNext.Server.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AspireNext.Server.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CanvasSize> CanvasSizes => Set<CanvasSize>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Return> Returns => Set<Return>();
    public DbSet<ReturnItem> ReturnItems => Set<ReturnItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>()
            .Property(p => p.Orientation)
            .HasConversion<string>();

        modelBuilder.Entity<CanvasSize>(size =>
        {
            size.Property(s => s.WidthCm).HasPrecision(6, 2);
            size.Property(s => s.HeightCm).HasPrecision(6, 2);
            size.Property(s => s.Price).HasPrecision(10, 2);
            size.Property(s => s.PriceUsd).HasPrecision(10, 2);
            size.Property(s => s.GelatoFormat).HasMaxLength(50);
        });

        modelBuilder.Entity<Order>()
            .HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId);

        modelBuilder.Entity<Order>()
            .Property(o => o.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Order>()
            .Property(o => o.Currency)
            .HasMaxLength(3);

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.StripeCheckoutSessionId);

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.GelatoOrderId);

        modelBuilder.Entity<Order>()
            .OwnsOne(o => o.ShippingAddress);

        modelBuilder.Entity<Order>()
            .Property(o => o.FulfillmentStatus)
            .HasConversion<string>();

        modelBuilder.Entity<OrderItem>()
            .Property(i => i.Price)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Return>()
            .HasMany(r => r.Items)
            .WithOne()
            .HasForeignKey(i => i.ReturnId);

        modelBuilder.Entity<Return>()
            .Property(r => r.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Return>()
            .Property(r => r.RefundAmount)
            .HasPrecision(10, 2);
    }
}
