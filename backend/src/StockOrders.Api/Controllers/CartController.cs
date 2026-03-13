using MediatR;
using Microsoft.AspNetCore.Mvc;
using StockOrders.Application.Common;
using StockOrders.Application.Features.Cart.Commands.AddToCart;
using StockOrders.Application.Features.Cart.Commands.Checkout;
using StockOrders.Application.Features.Cart.Commands.ClearCart;
using StockOrders.Application.Features.Cart.Commands.RemoveCartItem;
using StockOrders.Application.Features.Cart.Commands.UpdateCartItem;
using StockOrders.Application.Features.Cart.Queries.GetCart;

namespace StockOrders.Api.Controllers;

[ApiController]
[Route("api/cart")]
public class CartController(IMediator mediator) : ControllerBase
{
    [HttpGet("{sessionId}")]
    public async Task<IActionResult> GetCart(string sessionId)
    {
        var result = await mediator.Send(new GetCartQuery(sessionId));
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    [HttpPost("{sessionId}/items")]
    public async Task<IActionResult> AddToCart(string sessionId, [FromBody] AddToCartRequest request)
    {
        var result = await mediator.Send(new AddToCartCommand(request.ProductId, request.Quantity, sessionId));
        return result.IsSuccess ? Ok() : MapError(result.Error);
    }

    [HttpPut("{sessionId}/items/{productId}")]
    public async Task<IActionResult> UpdateCartItem(string sessionId, Guid productId, [FromBody] UpdateCartItemRequest request)
    {
        var result = await mediator.Send(new UpdateCartItemCommand(productId, request.NewQuantity, sessionId));
        return result.IsSuccess ? Ok() : MapError(result.Error);
    }

    [HttpDelete("{sessionId}/items/{cartItemId}")]
    public async Task<IActionResult> RemoveCartItem(string sessionId, Guid cartItemId)
    {
        var result = await mediator.Send(new RemoveCartItemCommand(cartItemId, sessionId));
        return result.IsSuccess ? Ok() : MapError(result.Error);
    }

    [HttpDelete("{sessionId}/items")]
    public async Task<IActionResult> ClearCart(string sessionId)
    {
        var result = await mediator.Send(new ClearCartCommand(sessionId));
        return result.IsSuccess ? Ok() : MapError(result.Error);
    }

    [HttpPost("{sessionId}/checkout")]
    public async Task<IActionResult> Checkout(string sessionId)
    {
        var result = await mediator.Send(new CheckoutCommand(sessionId));
        return result.IsSuccess ? Ok(result.Value) : MapError(result.Error);
    }

    private ObjectResult MapError(Error error) => error.Type switch
    {
        ErrorType.NotFound => NotFound(new { error.Code, error.Description }),
        ErrorType.Conflict => Conflict(new { error.Code, error.Description }),
        ErrorType.Validation => BadRequest(new { error.Code, error.Description }),
        _ => Problem(error.Description)
    };
}

public record AddToCartRequest(Guid ProductId, int Quantity);
public record UpdateCartItemRequest(int NewQuantity);
