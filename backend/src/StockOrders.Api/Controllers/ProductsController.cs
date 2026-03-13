using MediatR;
using Microsoft.AspNetCore.Mvc;
using StockOrders.Application.Features.Products.Queries.GetProducts;

namespace StockOrders.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var result = await _mediator.Send(new GetProductsQuery());
        return Ok(result);
    }
}
