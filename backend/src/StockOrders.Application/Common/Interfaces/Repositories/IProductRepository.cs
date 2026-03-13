using StockOrders.Domain.Entities;

namespace StockOrders.Application.Common.Interfaces.Repositories;

public interface IProductRepository
{
    Task<List<Product>> GetAllWithStockAsync(CancellationToken cancellationToken = default);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
