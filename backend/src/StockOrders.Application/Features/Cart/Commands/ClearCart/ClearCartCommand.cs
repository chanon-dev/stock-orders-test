using MediatR;
using StockOrders.Application.Common;
using StockOrders.Application.Common.Interfaces;

namespace StockOrders.Application.Features.Cart.Commands.ClearCart;

public record ClearCartCommand(string SessionId) : IRequest<Result>;

public class ClearCartCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<ClearCartCommand, Result>
{
    public async Task<Result> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        var cartItems = await unitOfWork.CartItems.GetBySessionAsync(request.SessionId, cancellationToken);

        foreach (var item in cartItems)
        {
            var stock = await unitOfWork.Stocks.GetByProductIdAsync(item.ProductId, cancellationToken);

            if (stock != null)
                stock.Quantity += item.Quantity;
        }

        unitOfWork.CartItems.RemoveRange(cartItems);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
