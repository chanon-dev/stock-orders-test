using MediatR;
using Microsoft.AspNetCore.Mvc;
using StockOrders.Application.Features.Cart.Commands.AddToCart;
using StockOrders.Application.Features.Cart.Commands.Checkout;
using StockOrders.Application.Features.Cart.Commands.ClearCart;
using StockOrders.Application.Features.Cart.Commands.RemoveCartItem;
using StockOrders.Application.Features.Cart.Commands.UpdateCartItem;
using StockOrders.Application.Features.Cart.Queries.GetCart;

namespace StockOrders.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly IMediator _mediator;

    public CartController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{sessionId}")]
    public async Task<IActionResult> GetCart(string sessionId)
    {
        var result = await _mediator.Send(new GetCartQuery(sessionId));
        return Ok(result);
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result) return BadRequest("Unable to add item to cart (Check stock).");
        return Ok(new { success = true });
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateCart([FromBody] UpdateCartItemCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result) return BadRequest("Unable to update cart (Check stock).");
        return Ok(new { success = true });
    }

    [HttpDelete("item/{cartItemId}")]
    public async Task<IActionResult> RemoveCartItem(Guid cartItemId, [FromQuery] string sessionId)
    {
        var result = await _mediator.Send(new RemoveCartItemCommand(cartItemId, sessionId));
        if (!result) return BadRequest("Item not found.");
        return Ok(new { success = true });
    }

    [HttpDelete("clear/{sessionId}")]
    public async Task<IActionResult> ClearCart(string sessionId)
    {
        await _mediator.Send(new ClearCartCommand(sessionId));
        return Ok(new { success = true });
    }

    [HttpPost("checkout/{sessionId}")]
    public async Task<IActionResult> Checkout(string sessionId)
    {
        var result = await _mediator.Send(new CheckoutCommand(sessionId));
        return Ok(result);
    }
}
