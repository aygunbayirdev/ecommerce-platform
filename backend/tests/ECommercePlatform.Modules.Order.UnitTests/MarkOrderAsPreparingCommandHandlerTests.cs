using ECommercePlatform.Modules.Order.Application.Abstractions;
using ECommercePlatform.Modules.Order.Application.Orders;
using ECommercePlatform.Modules.Order.Domain;
using ECommercePlatform.SharedKernel;
using Moq;

namespace ECommercePlatform.Modules.Order.UnitTests;

public sealed class MarkOrderAsPreparingCommandHandlerTests
{
    private readonly Mock<IOrderWriteRepository> _orderWriteRepository = new();
    private readonly MarkOrderAsPreparingCommandHandler _handler;

    public MarkOrderAsPreparingCommandHandlerTests()
    {
        _handler = new MarkOrderAsPreparingCommandHandler(_orderWriteRepository.Object);
    }

    private static Domain.Order CreatePaidOrder()
    {
        var items = new List<OrderItemSnapshot> { new(Guid.NewGuid(), "Telefon", "SKU-1", 100m, 1) };
        var order = Domain.Order.Create(
            Guid.NewGuid(), "Ayşe Yılmaz", "5551234567", "İstanbul", "Kadıköy", "Bir sokak No:1", "34000", items);
        order.MarkReadyForPayment();
        order.MarkAsPaid();

        return order;
    }

    [Fact]
    public async Task Handle_ShouldMarkAsPreparing_WhenOrderIsPaid()
    {
        var order = CreatePaidOrder();
        _orderWriteRepository
            .Setup(r => r.GetByIdWithItemsAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _handler.Handle(new MarkOrderAsPreparingCommand(order.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Preparing, order.Status);
        _orderWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenOrderDoesNotExist()
    {
        var orderId = Guid.NewGuid();
        _orderWriteRepository
            .Setup(r => r.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Order?)null);

        var result = await _handler.Handle(new MarkOrderAsPreparingCommand(orderId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Handle_ShouldReturnConflictWithoutSaving_WhenOrderIsNotPaid()
    {
        var items = new List<OrderItemSnapshot> { new(Guid.NewGuid(), "Telefon", "SKU-1", 100m, 1) };
        var order = Domain.Order.Create(
            Guid.NewGuid(), "Ayşe Yılmaz", "5551234567", "İstanbul", "Kadıköy", "Bir sokak No:1", "34000", items);
        order.MarkReadyForPayment();

        _orderWriteRepository
            .Setup(r => r.GetByIdWithItemsAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _handler.Handle(new MarkOrderAsPreparingCommand(order.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        _orderWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
