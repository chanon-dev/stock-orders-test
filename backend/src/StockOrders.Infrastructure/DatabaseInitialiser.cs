using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StockOrders.Infrastructure.Persistence;
using StockOrders.Infrastructure.Persistence.Seed;

namespace StockOrders.Infrastructure;

public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync();

            if (app.Environment.IsDevelopment())
            {
                await DbInitializer.SeedAsync(context);
            }
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();
            logger.LogError(ex, "An error occurred during database migration or seeding.");
            throw;
        }
    }
}
