using ECommercePlatform.Modules.Inventory.Application.StockItems;
using ECommercePlatform.Modules.Order.Application.Abstractions;
using ECommercePlatform.Modules.Order.Application.Orders;
using ECommercePlatform.Modules.Order.Domain;
using MediatR;
using Moq;

namespace ECommercePlatform.Modules.Order.UnitTests;

public sealed class CancelMyOrderCommandHandlerTests
{
    private readonly Mock<IOrderWriteRepository> _orderWriteRepository = new();
    private readonly Mock<ISender> _sender = new();
    private readonly CancelMyOrderCommandHandler _handler;

    public CancelMyOrderCommandHandlerTests()
    {
        _handler = new CancelMyOrderCommandHandler(_orderWriteRepository.Object, _sender.Object);
    }

    private static Domain.Order CreatePaymentPendingOrder(Guid userId, Guid variantId, int quantity)
    {
        var items = new List<OrderItemSnapshot> { new(variantId, "Telefon", "SKU-1", 100m, quantity) };
        var order = Domain.Order.Create(
            userId, "Ayşe Yılmaz", "5551234567", "İstanbul", "Kadıköy", "Bir sokak No:1", "34000", items);
        order.MarkReadyForPayment();

        return order;
    }

    [Fact]
    public async Task Handle_ShouldReleaseReservedStock_WhenCancellingFromPaymentPending()
    {
        var userId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var order = CreatePaymentPendingOrder(userId, variantId, 3);

        _orderWriteRepository
            .Setup(r => r.GetByIdWithItemsAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _handler.Handle(new CancelMyOrderCommand(userId, order.Id, "Vazgeçtim"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        _sender.Verify(
            s => s.Send(
                It.Is<ReleaseStockCommand>(c => c.Items.Single().ProductVariantId == variantId && c.Items.Single().Quantity == 3),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotReleaseStock_WhenCancellingAnAlreadyPaidOrder()
    {
        var userId = Guid.NewGuid();
        var order = CreatePaymentPendingOrder(userId, Guid.NewGuid(), 3);
        order.MarkAsPaid();

        _orderWriteRepository
            .Setup(r => r.GetByIdWithItemsAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _handler.Handle(new CancelMyOrderCommand(userId, order.Id, "Vazgeçtim"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _sender.Verify(s => s.Send(It.IsAny<ReleaseStockCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenOrderDoesNotBelongToUser()
    {
        var order = CreatePaymentPendingOrder(Guid.NewGuid(), Guid.NewGuid(), 1);

        _orderWriteRepository
            .Setup(r => r.GetByIdWithItemsAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _handler.Handle(new CancelMyOrderCommand(Guid.NewGuid(), order.Id, "Vazgeçtim"), CancellationToken.None);

        Assert.True(result.IsFailure);
        _sender.Verify(s => s.Send(It.IsAny<ReleaseStockCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
