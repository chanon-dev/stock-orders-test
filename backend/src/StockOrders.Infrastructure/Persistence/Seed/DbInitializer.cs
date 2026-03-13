using Microsoft.EntityFrameworkCore;
using StockOrders.Domain.Entities;

namespace StockOrders.Infrastructure.Persistence.Seed;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Products.AnyAsync()) return;

        var products = new List<Product>
        {
            new Product { Name = "iPhone 15 Pro", Price = 999.00m, ImageUrl = "https://example.com/iphone15.jpg" },
            new Product { Name = "MacBook Air M3", Price = 1299.00m, ImageUrl = "https://example.com/macbookm3.jpg" },
            new Product { Name = "AirPods Pro 2", Price = 249.00m, ImageUrl = "https://example.com/airpods.jpg" },
            new Product { Name = "Apple Watch Ultra 2", Price = 799.00m, ImageUrl = "https://example.com/watchultra.jpg" },
            new Product { Name = "iPad Pro M2", Price = 1099.00m, ImageUrl = "https://example.com/ipadpro.jpg" }
        };

        foreach (var product in products)
        {
            context.Products.Add(product);
            context.Stocks.Add(new Stock { ProductId = product.Id, Quantity = 10 }); // Start with 10 each
        }

        await context.SaveChangesAsync();
    }
}
