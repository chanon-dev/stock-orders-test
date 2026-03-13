using MediatR;
using StockOrders.Application.Common.Interfaces;

namespace StockOrders.Application.Features.Cart.Commands.UpdateCartItem;

public record UpdateCartItemCommand(Guid ProductId, int NewQuantity, string SessionId) : IRequest<bool>;

public class UpdateCartItemCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateCartItemCommand, bool>
{
    public async Task<bool> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        var cartItem = await unitOfWork.CartItems.GetItemByProductAndSessionAsync(request.ProductId, request.SessionId, cancellationToken);

        if (cartItem == null) return false;

        var stock = await unitOfWork.Stocks.GetByProductIdAsync(request.ProductId, cancellationToken);

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
            unitOfWork.CartItems.Remove(cartItem);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
