using MediatR;
using StockOrders.Application.Common;
using StockOrders.Application.Common.Interfaces;

namespace StockOrders.Application.Features.Cart.Queries.GetCart;

public record GetCartQuery(string SessionId) : IRequest<Result<CartDto>>;

public record CartDto(List<CartItemDto> Items, decimal Total);
public record CartItemDto(Guid Id, Guid ProductId, string ProductName, int Quantity, decimal Price, decimal SubTotal);

public class GetCartQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetCartQuery, Result<CartDto>>
{
    public async Task<Result<CartDto>> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var cartItems = await unitOfWork.CartItems.GetBySessionWithProductAsync(request.SessionId, cancellationToken);

        var items = cartItems.Select(c => new CartItemDto(
            c.Id,
            c.ProductId,
            c.Product!.Name,
            c.Quantity,
            c.Product.Price,
            c.Quantity * c.Product.Price
        )).ToList();

        return new CartDto(items, items.Sum(i => i.SubTotal));
    }
}
