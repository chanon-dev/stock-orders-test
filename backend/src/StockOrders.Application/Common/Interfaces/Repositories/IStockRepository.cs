using StockOrders.Domain.Entities;

namespace StockOrders.Application.Common.Interfaces.Repositories;

public interface IStockRepository
{
    Task<Stock?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
}
