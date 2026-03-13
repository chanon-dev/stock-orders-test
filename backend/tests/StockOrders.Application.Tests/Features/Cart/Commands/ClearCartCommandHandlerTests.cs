using FluentAssertions;
using Moq;
using Xunit;
using StockOrders.Application.Common.Interfaces;
using StockOrders.Application.Common.Interfaces.Repositories;
using StockOrders.Application.Features.Cart.Commands.ClearCart;
using StockOrders.Domain.Entities;

namespace StockOrders.Application.Tests.Features.Cart.Commands;

public class ClearCartCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IStockRepository> _stockRepoMock;
    private readonly Mock<ICartRepository> _cartRepoMock;
    private readonly ClearCartCommandHandler _handler;

    public ClearCartCommandHandlerTests()
    {
        _stockRepoMock = new Mock<IStockRepository>();
        _cartRepoMock = new Mock<ICartRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(u => u.Stocks).Returns(_stockRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.CartItems).Returns(_cartRepoMock.Object);
        _handler = new ClearCartCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCartHasItems_RestoresStockAndRemovesAllItems()
    {
        // Arrange
        const string sessionId = "session-1";
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();

        var stock1 = new Stock { ProductId = productId1, Quantity = 0 };
        var stock2 = new Stock { ProductId = productId2, Quantity = 3 };

        var cartItems = new List<CartItem>
        {
            new() { Id = Guid.NewGuid(), ProductId = productId1, Quantity = 2, SessionId = sessionId },
            new() { Id = Guid.NewGuid(), ProductId = productId2, Quantity = 1, SessionId = sessionId }
        };

        _cartRepoMock
            .Setup(r => r.GetBySessionAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartItems);
        _stockRepoMock
            .Setup(r => r.GetByProductIdAsync(productId1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stock1);
        _stockRepoMock
            .Setup(r => r.GetByProductIdAsync(productId2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stock2);
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(new ClearCartCommand(sessionId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        stock1.Quantity.Should().Be(2); // 0 + 2
        stock2.Quantity.Should().Be(4); // 3 + 1
        _cartRepoMock.Verify(r => r.RemoveRange(cartItems), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCartIsEmpty_SucceedsWithoutModifyingStock()
    {
        // Arrange
        const string sessionId = "session-empty";

        _cartRepoMock
            .Setup(r => r.GetBySessionAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(new ClearCartCommand(sessionId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _stockRepoMock.Verify(r => r.GetByProductIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _cartRepoMock.Verify(r => r.RemoveRange(It.Is<IEnumerable<CartItem>>(l => !l.Any())), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenStockNotFoundForItem_StillRemovesCartItems()
    {
        // Arrange
        const string sessionId = "session-1";
        var productId = Guid.NewGuid();

        var cartItems = new List<CartItem>
        {
            new() { Id = Guid.NewGuid(), ProductId = productId, Quantity = 2, SessionId = sessionId }
        };

        _cartRepoMock
            .Setup(r => r.GetBySessionAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartItems);
        _stockRepoMock
            .Setup(r => r.GetByProductIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stock?)null);
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(new ClearCartCommand(sessionId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _cartRepoMock.Verify(r => r.RemoveRange(cartItems), Times.Once);
    }
}
