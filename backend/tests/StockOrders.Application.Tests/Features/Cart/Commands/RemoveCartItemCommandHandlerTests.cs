using FluentAssertions;
using Moq;
using Xunit;
using StockOrders.Application.Common;
using StockOrders.Application.Common.Interfaces;
using StockOrders.Application.Common.Interfaces.Repositories;
using StockOrders.Application.Features.Cart.Commands.RemoveCartItem;
using StockOrders.Domain.Entities;

namespace StockOrders.Application.Tests.Features.Cart.Commands;

public class RemoveCartItemCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IStockRepository> _stockRepoMock;
    private readonly Mock<ICartRepository> _cartRepoMock;
    private readonly RemoveCartItemCommandHandler _handler;

    public RemoveCartItemCommandHandlerTests()
    {
        _stockRepoMock = new Mock<IStockRepository>();
        _cartRepoMock = new Mock<ICartRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(u => u.Stocks).Returns(_stockRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.CartItems).Returns(_cartRepoMock.Object);
        _handler = new RemoveCartItemCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCartItemExists_RemovesItemAndRestoresStock()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        const string sessionId = "session-1";

        var cartItem = new CartItem { Id = itemId, ProductId = productId, Quantity = 3, SessionId = sessionId };
        var stock = new Stock { ProductId = productId, Quantity = 7 };

        _cartRepoMock
            .Setup(r => r.GetItemByIdAsync(itemId, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartItem);
        _stockRepoMock
            .Setup(r => r.GetByProductIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stock);
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(new RemoveCartItemCommand(itemId, sessionId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        stock.Quantity.Should().Be(10); // 7 + 3 restored
        _cartRepoMock.Verify(r => r.Remove(cartItem), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCartItemNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        const string sessionId = "session-1";

        _cartRepoMock
            .Setup(r => r.GetItemByIdAsync(itemId, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CartItem?)null);

        // Act
        var result = await _handler.Handle(new RemoveCartItemCommand(itemId, sessionId), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("CartItem.NotFound");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenStockNotFound_StillRemovesCartItem()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        const string sessionId = "session-1";

        var cartItem = new CartItem { Id = itemId, ProductId = productId, Quantity = 2, SessionId = sessionId };

        _cartRepoMock
            .Setup(r => r.GetItemByIdAsync(itemId, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartItem);
        _stockRepoMock
            .Setup(r => r.GetByProductIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stock?)null);
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(new RemoveCartItemCommand(itemId, sessionId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _cartRepoMock.Verify(r => r.Remove(cartItem), Times.Once);
    }
}
