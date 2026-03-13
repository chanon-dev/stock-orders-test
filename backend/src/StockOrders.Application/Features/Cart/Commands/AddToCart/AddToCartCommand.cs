using MediatR;
using Microsoft.EntityFrameworkCore;
using StockOrders.Application.Common.Interfaces;
using StockOrders.Domain.Entities;

namespace StockOrders.Application.Features.Cart.Commands.AddToCart;

public record AddToCartCommand(Guid ProductId, int Quantity, string SessionId) : IRequest<bool>;

public class AddToCartCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<AddToCartCommand, bool>
{
    public async Task<bool> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Check Stock
            var stock = await unitOfWork.Stocks.GetByProductIdAsync(request.ProductId, cancellationToken);

            if (stock == null || stock.Quantity < request.Quantity)
            {
                return false; // Not enough stock
            }

            // 2. Decrement Stock (RowVersion will detect concurrent updates)
            stock.Quantity -= request.Quantity;

            // 3. Update or Add CartItem
            var existingItem = await unitOfWork.CartItems.GetItemByProductAndSessionAsync(request.ProductId, request.SessionId, cancellationToken);

            if (existingItem != null)
            {
                existingItem.Quantity += request.Quantity;
            }
            else
            {
                unitOfWork.CartItems.Add(new CartItem
                {
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    SessionId = request.SessionId
                });
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            // Another request modified the stock simultaneously — treat as out of stock
            return false;
        }
    }
}
