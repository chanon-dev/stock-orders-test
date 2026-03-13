using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Moq;
using StockOrders.Application.Common;
using StockOrders.Application.Common.Interfaces;
using StockOrders.Application.Common.Interfaces.Repositories;
using StockOrders.Application.Features.Cart.Commands.AddToCart;
using StockOrders.Domain.Entities;

namespace StockOrders.Application.Tests.Features.Cart.Commands;

public class AddToCartCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IStockRepository> _stockRepoMock;
    private readonly Mock<ICartRepository> _cartRepoMock;
    private readonly AddToCartCommandHandler _handler;

    public AddToCartCommandHandlerTests()
    {
        _stockRepoMock = new Mock<IStockRepository>();
        _cartRepoMock = new Mock<ICartRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(u => u.Stocks).Returns(_stockRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.CartItems).Returns(_cartRepoMock.Object);
        _handler = new AddToCartCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenStockSufficient_NewItem_AddsToCartAndDecrementsStock()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var stock = new Stock { ProductId = productId, Quantity = 10 };
        var command = new AddToCartCommand(productId, 3, "session-1");

        _stockRepoMock
            .Setup(r => r.GetByProductIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stock);
        _cartRepoMock
            .Setup(r => r.GetItemByProductAndSessionAsync(productId, "session-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((CartItem?)null);
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        stock.Quantity.Should().Be(7); // 10 - 3
        _cartRepoMock.Verify(r => r.Add(It.Is<CartItem>(c =>
            c.ProductId == productId &&
            c.Quantity == 3 &&
            c.SessionId == "session-1"
        )), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenItemAlreadyInCart_IncrementsQuantityAndDecrementsStock()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var stock = new Stock { ProductId = productId, Quantity = 5 };
        var existingItem = new CartItem { Id = Guid.NewGuid(), ProductId = productId, Quantity = 2, SessionId = "session-1" };
        var command = new AddToCartCommand(productId, 2, "session-1");

        _stockRepoMock
            .Setup(r => r.GetByProductIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stock);
        _cartRepoMock
            .Setup(r => r.GetItemByProductAndSessionAsync(productId, "session-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingItem);
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingItem.Quantity.Should().Be(4); // 2 + 2
        stock.Quantity.Should().Be(3); // 5 - 2
        _cartRepoMock.Verify(r => r.Add(It.IsAny<CartItem>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenStockNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new AddToCartCommand(productId, 1, "session-1");

        _stockRepoMock
            .Setup(r => r.GetByProductIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stock?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("Stock.NotFound");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenInsufficientStock_ReturnsConflictError()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var stock = new Stock { ProductId = productId, Quantity = 2 };
        var command = new AddToCartCommand(productId, 5, "session-1");

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

    [Fact]
    public async Task Handle_WhenStockExactlyMatchesQuantity_Succeeds()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var stock = new Stock { ProductId = productId, Quantity = 3 };
        var command = new AddToCartCommand(productId, 3, "session-1");

        _stockRepoMock
            .Setup(r => r.GetByProductIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stock);
        _cartRepoMock
            .Setup(r => r.GetItemByProductAndSessionAsync(productId, "session-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((CartItem?)null);
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        stock.Quantity.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenConcurrencyExceptionThrown_ReturnsConflictError()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var stock = new Stock { ProductId = productId, Quantity = 10 };
        var command = new AddToCartCommand(productId, 1, "session-1");

        _stockRepoMock
            .Setup(r => r.GetByProductIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stock);
        _cartRepoMock
            .Setup(r => r.GetItemByProductAndSessionAsync(productId, "session-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((CartItem?)null);
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        result.Error.Code.Should().Be("Stock.ConcurrencyConflict");
    }
}
