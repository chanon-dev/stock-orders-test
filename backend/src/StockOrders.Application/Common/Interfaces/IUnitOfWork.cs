using StockOrders.Application.Common.Interfaces.Repositories;

namespace StockOrders.Application.Common.Interfaces;

public interface IUnitOfWork
{
    IProductRepository Products { get; }
    IStockRepository Stocks { get; }
    ICartRepository CartItems { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
