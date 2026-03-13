using MediatR;
using Microsoft.AspNetCore.Mvc;
using StockOrders.Application.Features.Products.Queries.GetProducts;

namespace StockOrders.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var result = await mediator.Send(new GetProductsQuery());
        return result.IsSuccess ? Ok(result.Value) : Problem();
    }
}
