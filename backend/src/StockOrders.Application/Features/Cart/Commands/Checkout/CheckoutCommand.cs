using MediatR;
using StockOrders.Application.Common.Interfaces;

namespace StockOrders.Application.Features.Cart.Commands.Checkout;

public record CheckoutCommand(string SessionId) : IRequest<CheckoutResultDto>;

public record CheckoutResultDto(decimal Total, int ItemCount);

public class CheckoutCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<CheckoutCommand, CheckoutResultDto>
{
    public async Task<CheckoutResultDto> Handle(CheckoutCommand request, CancellationToken cancellationToken)
    {
        var cartItems = await unitOfWork.CartItems.GetBySessionWithProductAsync(request.SessionId, cancellationToken);

        var total = cartItems.Sum(c => c.Quantity * (c.Product?.Price ?? 0));
        var itemCount = cartItems.Count;

        // Stock is already decremented when items were added — do NOT restore
        unitOfWork.CartItems.RemoveRange(cartItems);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CheckoutResultDto(total, itemCount);
    }
}
