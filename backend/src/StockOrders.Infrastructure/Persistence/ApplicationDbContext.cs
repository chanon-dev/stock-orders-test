using Microsoft.EntityFrameworkCore;
using StockOrders.Application.Common.Interfaces;
using StockOrders.Domain.Entities;

namespace StockOrders.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<CartItem> CartItems => Set<CartItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Product Configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Price).HasPrecision(18, 2);
        });

        // Stock Configuration (One-to-One with Product)
        modelBuilder.Entity<Stock>(entity =>
        {
            entity.HasKey(e => e.ProductId);
            entity.HasOne(e => e.Product)
                  .WithOne(p => p.Stock)
                  .HasForeignKey<Stock>(e => e.ProductId);
        });

        // CartItem Configuration
        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId);
        });
    }
}
