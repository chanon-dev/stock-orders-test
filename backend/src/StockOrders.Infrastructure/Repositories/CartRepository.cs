using Microsoft.EntityFrameworkCore;
using StockOrders.Application.Common.Interfaces.Repositories;
using StockOrders.Domain.Entities;
using StockOrders.Infrastructure.Persistence;

namespace StockOrders.Infrastructure.Repositories;

public class CartRepository(ApplicationDbContext context) : ICartRepository
{
    public Task<List<CartItem>> GetBySessionAsync(string sessionId, CancellationToken cancellationToken = default)
        => context.CartItems
            .Where(c => c.SessionId == sessionId)
            .ToListAsync(cancellationToken);

    public Task<List<CartItem>> GetBySessionWithProductAsync(string sessionId, CancellationToken cancellationToken = default)
        => context.CartItems
            .Include(c => c.Product)
            .Where(c => c.SessionId == sessionId)
            .ToListAsync(cancellationToken);

    public Task<CartItem?> GetItemByIdAsync(Guid itemId, string sessionId, CancellationToken cancellationToken = default)
        => context.CartItems
            .FirstOrDefaultAsync(c => c.Id == itemId && c.SessionId == sessionId, cancellationToken);

    public Task<CartItem?> GetItemByProductAndSessionAsync(Guid productId, string sessionId, CancellationToken cancellationToken = default)
        => context.CartItems
            .FirstOrDefaultAsync(c => c.ProductId == productId && c.SessionId == sessionId, cancellationToken);

    public void Add(CartItem item) => context.CartItems.Add(item);

    public void Remove(CartItem item) => context.CartItems.Remove(item);

    public void RemoveRange(IEnumerable<CartItem> items) => context.CartItems.RemoveRange(items);
}
