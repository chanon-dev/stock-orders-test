using MediatR;
using StockOrders.Application.Common.Interfaces;

namespace StockOrders.Application.Features.Cart.Commands.ClearCart;

public record ClearCartCommand(string SessionId) : IRequest<bool>;

public class ClearCartCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<ClearCartCommand, bool>
{
    public async Task<bool> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        var cartItems = await unitOfWork.CartItems.GetBySessionAsync(request.SessionId, cancellationToken);

        foreach (var item in cartItems)
        {
            var stock = await unitOfWork.Stocks.GetByProductIdAsync(item.ProductId, cancellationToken);

            if (stock != null)
            {
                stock.Quantity += item.Quantity; // Restore stock
            }
        }

        unitOfWork.CartItems.RemoveRange(cartItems);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
