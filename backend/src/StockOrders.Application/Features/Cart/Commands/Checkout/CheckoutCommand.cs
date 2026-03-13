using MediatR;
using StockOrders.Application.Common;
using StockOrders.Application.Common.Interfaces;

namespace StockOrders.Application.Features.Cart.Commands.Checkout;

public record CheckoutCommand(string SessionId) : IRequest<Result<CheckoutResultDto>>;

public record CheckoutResultDto(decimal Total, int ItemCount);

public class CheckoutCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<CheckoutCommand, Result<CheckoutResultDto>>
{
    public async Task<Result<CheckoutResultDto>> Handle(CheckoutCommand request, CancellationToken cancellationToken)
    {
        var cartItems = await unitOfWork.CartItems.GetBySessionWithProductAsync(request.SessionId, cancellationToken);

        if (cartItems.Count == 0)
            return Error.Validation("Cart.Empty", "Cannot checkout an empty cart.");

        var total = cartItems.Sum(c => c.Quantity * (c.Product?.Price ?? 0));
        var itemCount = cartItems.Count;

        // Stock is already decremented when items were added — do NOT restore
        unitOfWork.CartItems.RemoveRange(cartItems);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CheckoutResultDto(total, itemCount);
    }
}
