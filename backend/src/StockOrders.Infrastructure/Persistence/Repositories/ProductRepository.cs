using Microsoft.EntityFrameworkCore;
using StockOrders.Application.Common.Interfaces.Repositories;
using StockOrders.Domain.Entities;

namespace StockOrders.Infrastructure.Persistence.Repositories;

public class ProductRepository(ApplicationDbContext context) : IProductRepository
{
    public Task<List<Product>> GetAllWithStockAsync(CancellationToken cancellationToken = default)
        => context.Products
            .Include(p => p.Stock)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => context.Products
            .Include(p => p.Stock)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
}
