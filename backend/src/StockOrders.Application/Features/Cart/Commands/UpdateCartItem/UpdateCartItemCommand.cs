using MediatR;
using StockOrders.Application.Common;
using StockOrders.Application.Common.Interfaces;

namespace StockOrders.Application.Features.Cart.Commands.UpdateCartItem;

public record UpdateCartItemCommand(Guid ProductId, int NewQuantity, string SessionId) : IRequest<Result>;

public class UpdateCartItemCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateCartItemCommand, Result>
{
    public async Task<Result> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        var cartItem = await unitOfWork.CartItems.GetItemByProductAndSessionAsync(request.ProductId, request.SessionId, cancellationToken);

        if (cartItem == null)
            return Error.NotFound("CartItem.NotFound", "Cart item not found.");

        var stock = await unitOfWork.Stocks.GetByProductIdAsync(request.ProductId, cancellationToken);

        if (stock == null)
            return Error.NotFound("Stock.NotFound", $"Stock for product {request.ProductId} not found.");

        var difference = request.NewQuantity - cartItem.Quantity;

        if (difference > 0)
        {
            if (stock.Quantity < difference)
                return Error.Conflict("Stock.Insufficient", "Not enough stock available.");

            stock.Quantity -= difference;
        }
        else if (difference < 0)
        {
            stock.Quantity += Math.Abs(difference);
        }

        cartItem.Quantity = request.NewQuantity;

        if (cartItem.Quantity <= 0)
            unitOfWork.CartItems.Remove(cartItem);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
