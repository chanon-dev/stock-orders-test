using MediatR;
using Microsoft.EntityFrameworkCore;
using StockOrders.Application.Common.Interfaces;
using StockOrders.Domain.Entities;

namespace StockOrders.Application.Features.Cart.Commands.AddToCart;

public record AddToCartCommand(Guid ProductId, int Quantity, string SessionId) : IRequest<bool>;

public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public AddToCartCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        // 1. Check Stock
        var stock = await _context.Stocks
            .FirstOrDefaultAsync(s => s.ProductId == request.ProductId, cancellationToken);

        if (stock == null || stock.Quantity < request.Quantity)
        {
            return false; // Not enough stock
        }

        // 2. Decrement Stock
        stock.Quantity -= request.Quantity;

        // 3. Update or Add CartItem
        var existingItem = await _context.CartItems
            .FirstOrDefaultAsync(c => c.ProductId == request.ProductId && c.SessionId == request.SessionId, cancellationToken);

        if (existingItem != null)
        {
            existingItem.Quantity += request.Quantity;
        }
        else
        {
            _context.CartItems.Add(new CartItem
            {
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                SessionId = request.SessionId
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
