using AspireNext.Server.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AspireNext.Server.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Return> Returns => Set<Return>();
    public DbSet<ReturnItem> ReturnItems => Set<ReturnItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Order>()
            .HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId);

        modelBuilder.Entity<Order>()
            .Property(o => o.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.StripeCheckoutSessionId);

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
