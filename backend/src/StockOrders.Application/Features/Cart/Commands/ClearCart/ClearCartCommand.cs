using MediatR;
using Microsoft.EntityFrameworkCore;
using StockOrders.Application.Common.Interfaces;

namespace StockOrders.Application.Features.Cart.Commands.ClearCart;

public record ClearCartCommand(string SessionId) : IRequest<bool>;

public class ClearCartCommandHandler : IRequestHandler<ClearCartCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ClearCartCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        var cartItems = await _context.CartItems
            .Where(c => c.SessionId == request.SessionId)
            .ToListAsync(cancellationToken);

        foreach (var item in cartItems)
        {
            var stock = await _context.Stocks
                .FirstOrDefaultAsync(s => s.ProductId == item.ProductId, cancellationToken);
            
            if (stock != null)
            {
                stock.Quantity += item.Quantity; // Restore stock
            }
        }

        _context.CartItems.RemoveRange(cartItems);
        await _context.SaveChangesAsync(cancellationToken);
        
        return true;
    }
}
