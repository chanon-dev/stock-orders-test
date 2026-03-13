using StockOrders.Domain.Entities;

namespace StockOrders.Application.Common.Interfaces.Repositories;

public interface ICartRepository
{
    Task<List<CartItem>> GetBySessionAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<List<CartItem>> GetBySessionWithProductAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<CartItem?> GetItemByIdAsync(Guid itemId, string sessionId, CancellationToken cancellationToken = default);
    Task<CartItem?> GetItemByProductAndSessionAsync(Guid productId, string sessionId, CancellationToken cancellationToken = default);
    void Add(CartItem item);
    void Remove(CartItem item);
    void RemoveRange(IEnumerable<CartItem> items);
}
