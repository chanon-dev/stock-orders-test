using MediatR;
using Microsoft.EntityFrameworkCore;
using StockOrders.Application.Common;
using StockOrders.Application.Common.Interfaces;
using StockOrders.Domain.Entities;

namespace StockOrders.Application.Features.Cart.Commands.AddToCart;

public record AddToCartCommand(Guid ProductId, int Quantity, string SessionId) : IRequest<Result>;

public class AddToCartCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<AddToCartCommand, Result>
{
    public async Task<Result> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var stock = await unitOfWork.Stocks.GetByProductIdAsync(request.ProductId, cancellationToken);

            if (stock == null)
                return Error.NotFound("Stock.NotFound", $"Stock for product {request.ProductId} not found.");

            if (stock.Quantity < request.Quantity)
                return Error.Conflict("Stock.Insufficient", "Not enough stock available.");

            stock.Quantity -= request.Quantity;

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
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Error.Conflict("Stock.ConcurrencyConflict", "Stock was modified by another request. Please try again.");
        }
    }
}
