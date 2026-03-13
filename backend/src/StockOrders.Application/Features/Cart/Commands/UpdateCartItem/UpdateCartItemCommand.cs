using MediatR;
using Microsoft.EntityFrameworkCore;
using StockOrders.Application.Common.Interfaces;

namespace StockOrders.Application.Features.Cart.Commands.UpdateCartItem;

public record UpdateCartItemCommand(Guid ProductId, int NewQuantity, string SessionId) : IRequest<bool>;

public class UpdateCartItemCommandHandler : IRequestHandler<UpdateCartItemCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateCartItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(c => c.ProductId == request.ProductId && c.SessionId == request.SessionId, cancellationToken);
        
        if (cartItem == null) return false;

        var stock = await _context.Stocks
            .FirstOrDefaultAsync(s => s.ProductId == request.ProductId, cancellationToken);
        
        if (stock == null) return false;

        var difference = request.NewQuantity - cartItem.Quantity;

        if (difference > 0) // Want more
        {
            if (stock.Quantity < difference) return false; // Out of stock
            stock.Quantity -= difference;
        }
        else if (difference < 0) // Want less
        {
            stock.Quantity += Math.Abs(difference);
        }

        cartItem.Quantity = request.NewQuantity;
        
        if (cartItem.Quantity <= 0)
        {
            _context.CartItems.Remove(cartItem);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
