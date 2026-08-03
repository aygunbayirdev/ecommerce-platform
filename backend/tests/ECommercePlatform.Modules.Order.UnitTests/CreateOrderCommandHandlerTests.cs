using ECommercePlatform.Modules.Cart.Application.Carts;
using ECommercePlatform.Modules.Cart.Application.Dtos;
using ECommercePlatform.Modules.Identity.Application.Addresses;
using ECommercePlatform.Modules.Identity.Application.Dtos;
using ECommercePlatform.Modules.Inventory.Application.StockItems;
using ECommercePlatform.Modules.Order.Application.Abstractions;
using ECommercePlatform.Modules.Order.Application.Orders;
using ECommercePlatform.SharedKernel;
using MediatR;
using Moq;

namespace ECommercePlatform.Modules.Order.UnitTests;

public sealed class CreateOrderCommandHandlerTests
{
    private readonly Mock<IOrderWriteRepository> _orderWriteRepository = new();
    private readonly Mock<ISender> _sender = new();
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _handler = new CreateOrderCommandHandler(_orderWriteRepository.Object, _sender.Object);
    }

    private static AddressDto Address(Guid id) => new(
        id, "Ev", "Ayşe Yılmaz", "5551234567", "İstanbul", "Kadıköy", "Bir sokak No:1", "34000", true, DateTime.UtcNow);

    private void SetUpCart(Guid userId, Guid cartId, CartViewDto cartView)
    {
        _sender
            .Setup(s => s.Send(It.Is<GetOrCreateCartForUserCommand>(c => c.UserId == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(cartId));
        _sender
            .Setup(s => s.Send(It.Is<GetCartByIdQuery>(q => q.CartId == cartId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(cartView));
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationError_WhenCartIsEmpty()
    {
        var userId = Guid.NewGuid();
        var cartId = Guid.NewGuid();
        var command = new CreateOrderCommand(userId, Guid.NewGuid());

        SetUpCart(userId, cartId, new CartViewDto(cartId, userId, [], 0m));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        _orderWriteRepository.Verify(r => r.Add(It.IsAny<Domain.Order>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenAddressDoesNotBelongToUser()
    {
        var userId = Guid.NewGuid();
        var cartId = Guid.NewGuid();
        var command = new CreateOrderCommand(userId, Guid.NewGuid());

        var cartView = new CartViewDto(
            cartId, userId, [new CartItemViewDto(Guid.NewGuid(), 1, "Telefon", "SKU-1", 100m, null, 100m)], 100m);
        SetUpCart(userId, cartId, cartView);

        _sender
            .Setup(s => s.Send(It.Is<GetAddressesByUserIdQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IReadOnlyList<AddressDto>>([Address(Guid.NewGuid())]));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        _orderWriteRepository.Verify(r => r.Add(It.IsAny<Domain.Order>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCreateOrderReserveStockAndClearCart_OnHappyPath()
    {
        var userId = Guid.NewGuid();
        var cartId = Guid.NewGuid();
        var addressId = Guid.NewGuid();
        var command = new CreateOrderCommand(userId, addressId);

        var cartView = new CartViewDto(
            cartId, userId, [new CartItemViewDto(Guid.NewGuid(), 2, "Telefon", "SKU-1", 100m, null, 200m)], 200m);
        SetUpCart(userId, cartId, cartView);

        _sender
            .Setup(s => s.Send(It.Is<GetAddressesByUserIdQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IReadOnlyList<AddressDto>>([Address(addressId)]));
        _sender
            .Setup(s => s.Send(It.IsAny<ReserveStockCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _sender
            .Setup(s => s.Send(It.Is<ClearCartCommand>(c => c.CartId == cartId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _orderWriteRepository.Verify(r => r.Add(It.IsAny<Domain.Order>()), Times.Once);
        _orderWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        _sender.Verify(s => s.Send(It.Is<ClearCartCommand>(c => c.CartId == cartId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCancelOrderAndReturnConflict_WhenStockReservationFails()
    {
        var userId = Guid.NewGuid();
        var cartId = Guid.NewGuid();
        var addressId = Guid.NewGuid();
        var command = new CreateOrderCommand(userId, addressId);

        var cartView = new CartViewDto(
            cartId, userId, [new CartItemViewDto(Guid.NewGuid(), 5, "Telefon", "SKU-1", 100m, null, 500m)], 500m);
        SetUpCart(userId, cartId, cartView);

        _sender
            .Setup(s => s.Send(It.Is<GetAddressesByUserIdQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IReadOnlyList<AddressDto>>([Address(addressId)]));
        _sender
            .Setup(s => s.Send(It.IsAny<ReserveStockCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(Error.Conflict("StockItems.InsufficientStock", "Yetersiz stok.")));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        _orderWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        _sender.Verify(s => s.Send(It.IsAny<ClearCartCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
