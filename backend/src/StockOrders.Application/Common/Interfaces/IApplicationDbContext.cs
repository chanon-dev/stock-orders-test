using Microsoft.EntityFrameworkCore;
using StockOrders.Domain.Entities;

namespace StockOrders.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Product> Products { get; }
    DbSet<Stock> Stocks { get; }
    DbSet<CartItem> CartItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
