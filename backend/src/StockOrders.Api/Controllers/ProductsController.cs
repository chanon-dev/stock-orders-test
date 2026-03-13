using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockOrders.Infrastructure.Persistence;

namespace StockOrders.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _context.Products
            .Include(p => p.Stock)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Price,
                p.ImageUrl,
                StockQuantity = p.Stock != null ? p.Stock.Quantity : 0
            })
            .ToListAsync();

        return Ok(products);
    }
}
