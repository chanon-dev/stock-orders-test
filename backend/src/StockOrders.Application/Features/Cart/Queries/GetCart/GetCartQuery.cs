using MediatR;
using Microsoft.EntityFrameworkCore;
using StockOrders.Application.Common.Interfaces;

namespace StockOrders.Application.Features.Cart.Queries.GetCart;

public record GetCartQuery(string SessionId) : IRequest<CartDto>;

public record CartDto(List<CartItemDto> Items, decimal Total);
public record CartItemDto(Guid ProductId, string ProductName, int Quantity, decimal Price, decimal SubTotal);

public class GetCartQueryHandler : IRequestHandler<GetCartQuery, CartDto>
{
    private readonly IApplicationDbContext _context;

    public GetCartQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CartDto> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var items = await _context.CartItems
            .Include(c => c.Product)
            .Where(c => c.SessionId == request.SessionId)
            .Select(c => new CartItemDto(
                c.ProductId,
                c.Product!.Name,
                c.Quantity,
                c.Product.Price,
                c.Quantity * c.Product.Price
            ))
            .ToListAsync(cancellationToken);

        return new CartDto(items, items.Sum(i => i.SubTotal));
    }
}
