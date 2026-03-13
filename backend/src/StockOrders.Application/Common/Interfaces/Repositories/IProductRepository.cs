using StockOrders.Domain.Entities;

namespace StockOrders.Application.Common.Interfaces.Repositories;

public interface IProductRepository
{
    Task<List<Product>> GetAllWithStockAsync(CancellationToken cancellationToken = default);
}
