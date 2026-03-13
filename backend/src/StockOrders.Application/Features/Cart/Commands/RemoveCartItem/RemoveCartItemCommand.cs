using MediatR;
using StockOrders.Application.Common.Interfaces;

namespace StockOrders.Application.Features.Cart.Commands.RemoveCartItem;

public record RemoveCartItemCommand(Guid CartItemId, string SessionId) : IRequest<bool>;

public class RemoveCartItemCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<RemoveCartItemCommand, bool>
{
    public async Task<bool> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        var cartItem = await unitOfWork.CartItems.GetItemByIdAsync(request.CartItemId, request.SessionId, cancellationToken);

        if (cartItem == null) return false;

        var stock = await unitOfWork.Stocks.GetByProductIdAsync(cartItem.ProductId, cancellationToken);

        if (stock != null)
        {
            stock.Quantity += cartItem.Quantity; // Restore stock
        }

        unitOfWork.CartItems.Remove(cartItem);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
