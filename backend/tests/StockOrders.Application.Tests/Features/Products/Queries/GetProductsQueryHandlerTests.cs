using FluentAssertions;
using Moq;
using Xunit;
using StockOrders.Application.Common.Interfaces;
using StockOrders.Application.Common.Interfaces.Repositories;
using StockOrders.Application.Features.Products.Queries.GetProducts;
using StockOrders.Domain.Entities;

namespace StockOrders.Application.Tests.Features.Products.Queries;

public class GetProductsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly GetProductsQueryHandler _handler;

    public GetProductsQueryHandlerTests()
    {
        _productRepoMock = new Mock<IProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepoMock.Object);
        _handler = new GetProductsQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenProductsExist_ReturnsMappedDtos()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var products = new List<Product>
        {
            new()
            {
                Id = productId,
                Name = "iPhone 15 Pro",
                Price = 999.00m,
                ImageUrl = "https://example.com/iphone.jpg",
                Stock = new Stock { ProductId = productId, Quantity = 10 }
            }
        };

        _productRepoMock
            .Setup(r => r.GetAllWithStockAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _handler.Handle(new GetProductsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);

        var dto = result.Value[0];
        dto.Id.Should().Be(productId);
        dto.Name.Should().Be("iPhone 15 Pro");
        dto.Price.Should().Be(999.00m);
        dto.ImageUrl.Should().Be("https://example.com/iphone.jpg");
        dto.StockQuantity.Should().Be(10);
    }

    [Fact]
    public async Task Handle_WhenProductHasNoStock_ReturnsZeroStockQuantity()
    {
        // Arrange
        var products = new List<Product>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "AirPods Pro",
                Price = 249.00m,
                ImageUrl = "https://example.com/airpods.jpg",
                Stock = null
            }
        };

        _productRepoMock
            .Setup(r => r.GetAllWithStockAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _handler.Handle(new GetProductsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value[0].StockQuantity.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenNoProducts_ReturnsEmptyList()
    {
        // Arrange
        _productRepoMock
            .Setup(r => r.GetAllWithStockAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _handler.Handle(new GetProductsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenMultipleProducts_ReturnsAllMapped()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = Guid.NewGuid(), Name = "iPhone", Price = 999m, ImageUrl = "", Stock = new Stock { Quantity = 5 } },
            new() { Id = Guid.NewGuid(), Name = "MacBook", Price = 1299m, ImageUrl = "", Stock = new Stock { Quantity = 3 } },
        };

        _productRepoMock
            .Setup(r => r.GetAllWithStockAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _handler.Handle(new GetProductsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }
}
