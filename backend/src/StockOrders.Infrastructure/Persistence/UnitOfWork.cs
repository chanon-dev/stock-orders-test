using StockOrders.Application.Common.Interfaces;
using StockOrders.Application.Common.Interfaces.Repositories;
using StockOrders.Infrastructure.Persistence.Repositories;

namespace StockOrders.Infrastructure.Persistence;

public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    public IProductRepository Products { get; } = new ProductRepository(context);
    public IStockRepository Stocks { get; } = new StockRepository(context);
    public ICartRepository CartItems { get; } = new CartRepository(context);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}
