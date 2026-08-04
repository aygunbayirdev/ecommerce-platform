using ECommercePlatform.Modules.Order.Application.Orders;
using ECommercePlatform.Modules.Shipping.Application.Abstractions;
using ECommercePlatform.Modules.Shipping.Application.Shipments;
using ECommercePlatform.SharedKernel;
using MediatR;
using Moq;

namespace ECommercePlatform.Modules.Shipping.UnitTests;

public sealed class MarkShipmentDeliveredCommandHandlerTests
{
    private readonly Mock<IShipmentWriteRepository> _shipmentWriteRepository = new();
    private readonly Mock<ISender> _sender = new();
    private readonly MarkShipmentDeliveredCommandHandler _handler;

    public MarkShipmentDeliveredCommandHandlerTests()
    {
        _handler = new MarkShipmentDeliveredCommandHandler(_shipmentWriteRepository.Object, _sender.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenShipmentDoesNotExist()
    {
        _shipmentWriteRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Shipment?)null);

        var result = await _handler.Handle(new MarkShipmentDeliveredCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        _sender.Verify(s => s.Send(It.IsAny<MarkOrderAsDeliveredCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldMarkDeliveredAndDriveOrder_OnHappyPath()
    {
        var orderId = Guid.NewGuid();
        var shipment = Domain.Shipment.Create(orderId, "Yurtiçi Kargo", "YK123");

        _shipmentWriteRepository
            .Setup(r => r.GetByIdAsync(shipment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shipment);

        var result = await _handler.Handle(new MarkShipmentDeliveredCommand(shipment.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _shipmentWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _sender.Verify(
            s => s.Send(It.Is<MarkOrderAsDeliveredCommand>(c => c.OrderId == orderId), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
