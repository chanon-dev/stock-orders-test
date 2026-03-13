using FluentAssertions;
using Moq;
using Xunit;
using StockOrders.Application.Common;
using StockOrders.Application.Common.Interfaces;
using StockOrders.Application.Common.Interfaces.Repositories;
using StockOrders.Application.Features.Cart.Commands.UpdateCartItem;
using StockOrders.Domain.Entities;

namespace StockOrders.Application.Tests.Features.Cart.Commands;

public class UpdateCartItemCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IStockRepository> _stockRepoMock;
    private readonly Mock<ICartRepository> _cartRepoMock;
    private readonly UpdateCartItemCommandHandler _handler;

    public UpdateCartItemCommandHandlerTests()
    {
        _stockRepoMock = new Mock<IStockRepository>();
        _cartRepoMock = new Mock<ICartRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(u => u.Stocks).Returns(_stockRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.CartItems).Returns(_cartRepoMock.Object);
        _handler = new UpdateCartItemCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenIncreasingQuantity_DecrementsStock()
    {
        // Arrange
        var productId = Guid.NewGuid();
        const string sessionId = "session-1";

        var cartItem = new CartItem { Id = Guid.NewGuid(), ProductId = productId, Quantity = 2, SessionId = sessionId };
        var stock = new Stock { ProductId = productId, Quantity = 5 };
        var command = new UpdateCartItemCommand(productId, 4, sessionId); // increase 2 -> 4

        _cartRepoMock
            .Setup(r => r.GetItemByProductAndSessionAsync(productId, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartItem);
        _stockRepoMock
            .Setup(r => r.GetByProductIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stock);
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        cartItem.Quantity.Should().Be(4);
        stock.Quantity.Should().Be(3); // 5 - 2 (difference)
        _cartRepoMock.Verify(r => r.Remove(It.IsAny<CartItem>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenDecreasingQuantity_RestoresStock()
    {
        // Arrange
        var productId = Guid.NewGuid();
        const string sessionId = "session-1";

        var cartItem = new CartItem { Id = Guid.NewGuid(), ProductId = productId, Quantity = 5, SessionId = sessionId };
        var stock = new Stock { ProductId = productId, Quantity = 0 };
        var command = new UpdateCartItemCommand(productId, 3, sessionId); // decrease 5 -> 3

        _cartRepoMock
            .Setup(r => r.GetItemByProductAndSessionAsync(productId, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartItem);
        _stockRepoMock
            .Setup(r => r.GetByProductIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stock);
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        cartItem.Quantity.Should().Be(3);
        stock.Quantity.Should().Be(2); // 0 + 2 (returned difference)
    }

    [Fact]
    public async Task Handle_WhenNewQuantityIsZero_RemovesCartItem()
    {
        // Arrange
        var productId = Guid.NewGuid();
        const string sessionId = "session-1";

        var cartItem = new CartItem { Id = Guid.NewGuid(), ProductId = productId, Quantity = 3, SessionId = sessionId };
        var stock = new Stock { ProductId = productId, Quantity = 2 };
        var command = new UpdateCartItemCommand(productId, 0, sessionId);

        _cartRepoMock
            .Setup(r => r.GetItemByProductAndSessionAsync(productId, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartItem);
        _stockRepoMock
            .Setup(r => r.GetByProductIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stock);
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        stock.Quantity.Should().Be(5); // 2 + 3 (all returned)
        _cartRepoMock.Verify(r => r.Remove(cartItem), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNewQuantityIsNegative_RemovesCartItem()
    {
        // Arrange
        var productId = Guid.NewGuid();
        const string sessionId = "session-1";

        var cartItem = new CartItem { Id = Guid.NewGuid(), ProductId = productId, Quantity = 2, SessionId = sessionId };
        var stock = new Stock { ProductId = productId, Quantity = 1 };
        var command = new UpdateCartItemCommand(productId, -1, sessionId);

        _cartRepoMock
            .Setup(r => r.GetItemByProductAndSessionAsync(productId, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartItem);
        _stockRepoMock
            .Setup(r => r.GetByProductIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stock);
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _cartRepoMock.Verify(r => r.Remove(cartItem), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCartItemNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var productId = Guid.NewGuid();
        const string sessionId = "session-1";

        _cartRepoMock
            .Setup(r => r.GetItemByProductAndSessionAsync(productId, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CartItem?)null);

        // Act
        var result = await _handler.Handle(new UpdateCartItemCommand(productId, 3, sessionId), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("CartItem.NotFound");
    }

    [Fact]
    public async Task Handle_WhenStockNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var productId = Guid.NewGuid();
        const string sessionId = "session-1";

        var cartItem = new CartItem { Id = Guid.NewGuid(), ProductId = productId, Quantity = 1, SessionId = sessionId };

        _cartRepoMock
            .Setup(r => r.GetItemByProductAndSessionAsync(productId, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartItem);
        _stockRepoMock
            .Setup(r => r.GetByProductIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stock?)null);

        // Act
        var result = await _handler.Handle(new UpdateCartItemCommand(productId, 3, sessionId), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("Stock.NotFound");
    }

    [Fact]
    public async Task Handle_WhenInsufficientStockForIncrease_ReturnsConflictError()
    {
        // Arrange
        var productId = Guid.NewGuid();
        const string sessionId = "session-1";

        var cartItem = new CartItem { Id = Guid.NewGuid(), ProductId = productId, Quantity = 2, SessionId = sessionId };
        var stock = new Stock { ProductId = productId, Quantity = 1 };
        var command = new UpdateCartItemCommand(productId, 5, sessionId); // need 3 more, only 1 available

        _cartRepoMock
            .Setup(r => r.GetItemByProductAndSessionAsync(productId, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartItem);
        _stockRepoMock
            .Setup(r => r.GetByProductIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stock);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        result.Error.Code.Should().Be("Stock.Insufficient");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
