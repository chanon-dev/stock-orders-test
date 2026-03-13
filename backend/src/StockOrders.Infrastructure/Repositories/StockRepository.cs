using Microsoft.EntityFrameworkCore;
using StockOrders.Application.Common.Interfaces.Repositories;
using StockOrders.Domain.Entities;
using StockOrders.Infrastructure.Persistence;

namespace StockOrders.Infrastructure.Repositories;

public class StockRepository(ApplicationDbContext context) : IStockRepository
{
    public Task<Stock?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
        => context.Stocks
            .FirstOrDefaultAsync(s => s.ProductId == productId, cancellationToken);
}
