using ECommercePlatform.Modules.Order.Application.Dtos;
using ECommercePlatform.Modules.Order.Application.Orders;
using ECommercePlatform.Modules.Shipping.Application.Abstractions;
using ECommercePlatform.Modules.Shipping.Application.Shipments;
using ECommercePlatform.SharedKernel;
using MediatR;
using Moq;

namespace ECommercePlatform.Modules.Shipping.UnitTests;

public sealed class CreateShipmentCommandHandlerTests
{
    private readonly Mock<IShipmentWriteRepository> _shipmentWriteRepository = new();
    private readonly Mock<ISender> _sender = new();
    private readonly CreateShipmentCommandHandler _handler;

    public CreateShipmentCommandHandlerTests()
    {
        _handler = new CreateShipmentCommandHandler(_shipmentWriteRepository.Object, _sender.Object);
    }

    private static OrderDetailDto Order(string status) => new(
        Guid.NewGuid(), "ORD-1", Guid.NewGuid(), status, "Ayşe Yılmaz", "5551234567",
        "İstanbul", "Kadıköy", "Bir sokak No:1", "34000", null, 0m, DateTime.UtcNow, [], [], 100m);

    [Fact]
    public async Task Handle_ShouldReturnConflict_WhenOrderIsNotPreparing()
    {
        var orderId = Guid.NewGuid();
        var command = new CreateShipmentCommand(orderId, "Yurtiçi Kargo", "YK123");

        _sender
            .Setup(s => s.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Order("Paid")));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        _shipmentWriteRepository.Verify(r => r.Add(It.IsAny<Domain.Shipment>()), Times.Never);
        _sender.Verify(s => s.Send(It.IsAny<MarkOrderAsShippedCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnConflict_WhenShipmentAlreadyExists()
    {
        var orderId = Guid.NewGuid();
        var command = new CreateShipmentCommand(orderId, "Yurtiçi Kargo", "YK123");

        _sender
            .Setup(s => s.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Order("Preparing")));
        _shipmentWriteRepository
            .Setup(r => r.ExistsByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        _shipmentWriteRepository.Verify(r => r.Add(It.IsAny<Domain.Shipment>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCreateShipmentAndMarkOrderAsShipped_OnHappyPath()
    {
        var orderId = Guid.NewGuid();
        var command = new CreateShipmentCommand(orderId, "Yurtiçi Kargo", "YK123");

        _sender
            .Setup(s => s.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Order("Preparing")));
        _shipmentWriteRepository
            .Setup(r => r.ExistsByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _shipmentWriteRepository.Verify(r => r.Add(It.IsAny<Domain.Shipment>()), Times.Once);
        _shipmentWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _sender.Verify(
            s => s.Send(It.Is<MarkOrderAsShippedCommand>(c => c.OrderId == orderId), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
