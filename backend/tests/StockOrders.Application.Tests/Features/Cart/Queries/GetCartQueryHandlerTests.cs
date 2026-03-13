using FluentAssertions;
using Moq;
using Xunit;
using StockOrders.Application.Common.Interfaces;
using StockOrders.Application.Common.Interfaces.Repositories;
using StockOrders.Application.Features.Cart.Queries.GetCart;
using StockOrders.Domain.Entities;

namespace StockOrders.Application.Tests.Features.Cart.Queries;

public class GetCartQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICartRepository> _cartRepoMock;
    private readonly GetCartQueryHandler _handler;

    public GetCartQueryHandlerTests()
    {
        _cartRepoMock = new Mock<ICartRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(u => u.CartItems).Returns(_cartRepoMock.Object);
        _handler = new GetCartQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCartHasItems_ReturnsMappedCartDto()
    {
        // Arrange
        const string sessionId = "session-abc";
        var productId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        var cartItems = new List<CartItem>
        {
            new()
            {
                Id = itemId,
                ProductId = productId,
                Quantity = 2,
                SessionId = sessionId,
                Product = new Product { Id = productId, Name = "iPhone 15 Pro", Price = 999.00m, ImageUrl = "" }
            }
        };

        _cartRepoMock
            .Setup(r => r.GetBySessionWithProductAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartItems);

        // Act
        var result = await _handler.Handle(new GetCartQuery(sessionId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var cart = result.Value;
        cart.Items.Should().HaveCount(1);
        cart.Total.Should().Be(1998.00m); // 2 * 999

        var item = cart.Items[0];
        item.Id.Should().Be(itemId);
        item.ProductId.Should().Be(productId);
        item.ProductName.Should().Be("iPhone 15 Pro");
        item.Quantity.Should().Be(2);
        item.Price.Should().Be(999.00m);
        item.SubTotal.Should().Be(1998.00m);
    }

    [Fact]
    public async Task Handle_WhenCartIsEmpty_ReturnsEmptyCartWithZeroTotal()
    {
        // Arrange
        const string sessionId = "session-empty";

        _cartRepoMock
            .Setup(r => r.GetBySessionWithProductAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _handler.Handle(new GetCartQuery(sessionId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.Total.Should().Be(0m);
    }

    [Fact]
    public async Task Handle_WhenCartHasMultipleItems_CalculatesTotalCorrectly()
    {
        // Arrange
        const string sessionId = "session-multi";

        var cartItems = new List<CartItem>
        {
            new()
            {
                Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 1, SessionId = sessionId,
                Product = new Product { Id = Guid.NewGuid(), Name = "MacBook", Price = 1299.00m, ImageUrl = "" }
            },
            new()
            {
                Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 3, SessionId = sessionId,
                Product = new Product { Id = Guid.NewGuid(), Name = "AirPods", Price = 249.00m, ImageUrl = "" }
            }
        };

        _cartRepoMock
            .Setup(r => r.GetBySessionWithProductAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartItems);

        // Act
        var result = await _handler.Handle(new GetCartQuery(sessionId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Total.Should().Be(1299.00m + (3 * 249.00m)); // 2046
    }
}
