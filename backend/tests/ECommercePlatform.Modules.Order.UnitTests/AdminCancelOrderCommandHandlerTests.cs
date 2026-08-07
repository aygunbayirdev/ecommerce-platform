using ECommercePlatform.Modules.Inventory.Application.StockItems;
using ECommercePlatform.Modules.Order.Application.Abstractions;
using ECommercePlatform.Modules.Order.Application.Orders;
using ECommercePlatform.Modules.Order.Domain;
using ECommercePlatform.Modules.Promotion.Application.Coupons;
using ECommercePlatform.SharedKernel;
using MediatR;
using Moq;

namespace ECommercePlatform.Modules.Order.UnitTests;

public sealed class AdminCancelOrderCommandHandlerTests
{
    private readonly Mock<IOrderWriteRepository> _orderWriteRepository = new();
    private readonly Mock<ISender> _sender = new();
    private readonly AdminCancelOrderCommandHandler _handler;

    public AdminCancelOrderCommandHandlerTests()
    {
        _handler = new AdminCancelOrderCommandHandler(_orderWriteRepository.Object, _sender.Object);
    }

    private static Domain.Order CreatePaymentPendingOrder(Guid variantId, int quantity)
    {
        var items = new List<OrderItemSnapshot> { new(variantId, "Telefon", "SKU-1", 100m, quantity) };
        var order = Domain.Order.Create(
            Guid.NewGuid(), "Ayşe Yılmaz", "5551234567", "İstanbul", "Kadıköy", "Bir sokak No:1", "34000", items);
        order.MarkReadyForPayment();

        return order;
    }

    [Fact]
    public async Task Handle_ShouldReleaseReservedStock_WhenCancellingFromPaymentPending()
    {
        var variantId = Guid.NewGuid();
        var order = CreatePaymentPendingOrder(variantId, 3);

        _orderWriteRepository
            .Setup(r => r.GetByIdWithItemsAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _handler.Handle(new AdminCancelOrderCommand(order.Id, "Müşteri telefonla iptal istedi"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        _sender.Verify(
            s => s.Send(
                It.Is<ReleaseStockCommand>(c => c.Items.Single().ProductVariantId == variantId && c.Items.Single().Quantity == 3),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _sender.Verify(
            s => s.Send(It.Is<ReleaseCouponRedemptionCommand>(c => c.OrderId == order.Id), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotReleaseStock_WhenCancellingAnAlreadyPaidOrder()
    {
        var order = CreatePaymentPendingOrder(Guid.NewGuid(), 3);
        order.MarkAsPaid();

        _orderWriteRepository
            .Setup(r => r.GetByIdWithItemsAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _handler.Handle(new AdminCancelOrderCommand(order.Id, "Müşteri telefonla iptal istedi"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _sender.Verify(s => s.Send(It.IsAny<ReleaseStockCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        _sender.Verify(s => s.Send(It.IsAny<ReleaseCouponRedemptionCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenOrderDoesNotExist()
    {
        var orderId = Guid.NewGuid();
        _orderWriteRepository
            .Setup(r => r.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Order?)null);

        var result = await _handler.Handle(new AdminCancelOrderCommand(orderId, "Müşteri telefonla iptal istedi"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        _sender.Verify(s => s.Send(It.IsAny<ReleaseStockCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnConflict_WhenOrderAlreadyShipped()
    {
        var order = CreatePaymentPendingOrder(Guid.NewGuid(), 1);
        order.MarkAsPaid();
        order.MarkAsPreparing();
        order.MarkAsShipped();

        _orderWriteRepository
            .Setup(r => r.GetByIdWithItemsAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _handler.Handle(new AdminCancelOrderCommand(order.Id, "Müşteri telefonla iptal istedi"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }
}
