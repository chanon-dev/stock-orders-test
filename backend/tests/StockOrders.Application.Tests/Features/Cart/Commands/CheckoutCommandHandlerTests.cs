using FluentAssertions;
using Moq;
using Xunit;
using StockOrders.Application.Common;
using StockOrders.Application.Common.Interfaces;
using StockOrders.Application.Common.Interfaces.Repositories;
using StockOrders.Application.Features.Cart.Commands.Checkout;
using StockOrders.Domain.Entities;

namespace StockOrders.Application.Tests.Features.Cart.Commands;

public class CheckoutCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICartRepository> _cartRepoMock;
    private readonly CheckoutCommandHandler _handler;

    public CheckoutCommandHandlerTests()
    {
        _cartRepoMock = new Mock<ICartRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(u => u.CartItems).Returns(_cartRepoMock.Object);
        _handler = new CheckoutCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCartHasItems_ReturnsCheckoutResultAndClearsCart()
    {
        // Arrange
        const string sessionId = "session-1";
        var cartItems = new List<CartItem>
        {
            new()
            {
                Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 2, SessionId = sessionId,
                Product = new Product { Id = Guid.NewGuid(), Name = "iPhone", Price = 999.00m, ImageUrl = "" }
            },
            new()
            {
                Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 1, SessionId = sessionId,
                Product = new Product { Id = Guid.NewGuid(), Name = "AirPods", Price = 249.00m, ImageUrl = "" }
            }
        };

        _cartRepoMock
            .Setup(r => r.GetBySessionWithProductAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartItems);
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(new CheckoutCommand(sessionId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Total.Should().Be(2247.00m); // (2 * 999) + (1 * 249)
        result.Value.ItemCount.Should().Be(2);
        _cartRepoMock.Verify(r => r.RemoveRange(cartItems), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCartIsEmpty_ReturnsValidationError()
    {
        // Arrange
        const string sessionId = "session-empty";

        _cartRepoMock
            .Setup(r => r.GetBySessionWithProductAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _handler.Handle(new CheckoutCommand(sessionId), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
        result.Error.Code.Should().Be("Cart.Empty");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCheckout_DoesNotRestoreStock()
    {
        // Arrange - stock is NOT set up because checkout should not touch stock
        const string sessionId = "session-1";
        var mockStockRepo = new Mock<IStockRepository>();
        _unitOfWorkMock.Setup(u => u.Stocks).Returns(mockStockRepo.Object);

        var cartItems = new List<CartItem>
        {
            new()
            {
                Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 1, SessionId = sessionId,
                Product = new Product { Id = Guid.NewGuid(), Name = "MacBook", Price = 1299.00m, ImageUrl = "" }
            }
        };

        _cartRepoMock
            .Setup(r => r.GetBySessionWithProductAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartItems);
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(new CheckoutCommand(sessionId), CancellationToken.None);

        // Assert: stock repo must not be called during checkout
        mockStockRepo.Verify(r => r.GetByProductIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSingleItem_CalculatesTotalCorrectly()
    {
        // Arrange
        const string sessionId = "session-1";
        var cartItems = new List<CartItem>
        {
            new()
            {
                Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 3, SessionId = sessionId,
                Product = new Product { Id = Guid.NewGuid(), Name = "Watch", Price = 799.00m, ImageUrl = "" }
            }
        };

        _cartRepoMock
            .Setup(r => r.GetBySessionWithProductAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartItems);
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(new CheckoutCommand(sessionId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Total.Should().Be(2397.00m); // 3 * 799
        result.Value.ItemCount.Should().Be(1);
    }
}
