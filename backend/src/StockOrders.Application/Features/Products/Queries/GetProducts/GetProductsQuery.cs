using MediatR;
using StockOrders.Application.Common.Interfaces;

namespace StockOrders.Application.Features.Products.Queries.GetProducts;

public record GetProductsQuery : IRequest<List<ProductDto>>;

public record ProductDto(Guid Id, string Name, decimal Price, string ImageUrl, int StockQuantity);

public class GetProductsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetProductsQuery, List<ProductDto>>
{
    public async Task<List<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await unitOfWork.Products.GetAllWithStockAsync(cancellationToken);

        return products.Select(p => new ProductDto(
            p.Id,
            p.Name,
            p.Price,
            p.ImageUrl,
            p.Stock?.Quantity ?? 0
        )).ToList();
    }
}
