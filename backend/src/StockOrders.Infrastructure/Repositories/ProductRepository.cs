using Microsoft.EntityFrameworkCore;
using StockOrders.Application.Common.Interfaces.Repositories;
using StockOrders.Domain.Entities;
using StockOrders.Infrastructure.Persistence;

namespace StockOrders.Infrastructure.Repositories;

public class ProductRepository(ApplicationDbContext context) : IProductRepository
{
    public Task<List<Product>> GetAllWithStockAsync(CancellationToken cancellationToken = default)
        => context.Products
            .Include(p => p.Stock)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

}
