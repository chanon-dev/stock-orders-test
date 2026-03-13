using MediatR;
using StockOrders.Application.Common;
using StockOrders.Application.Common.Interfaces;

namespace StockOrders.Application.Features.Cart.Commands.RemoveCartItem;

public record RemoveCartItemCommand(Guid CartItemId, string SessionId) : IRequest<Result>;

public class RemoveCartItemCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<RemoveCartItemCommand, Result>
{
    public async Task<Result> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        var cartItem = await unitOfWork.CartItems.GetItemByIdAsync(request.CartItemId, request.SessionId, cancellationToken);

        if (cartItem == null)
            return Error.NotFound("CartItem.NotFound", "Cart item not found.");

        var stock = await unitOfWork.Stocks.GetByProductIdAsync(cartItem.ProductId, cancellationToken);

        if (stock != null)
            stock.Quantity += cartItem.Quantity;

        unitOfWork.CartItems.Remove(cartItem);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
