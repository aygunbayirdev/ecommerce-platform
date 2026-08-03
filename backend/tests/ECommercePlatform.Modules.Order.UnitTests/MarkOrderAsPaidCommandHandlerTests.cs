using ECommercePlatform.Modules.Inventory.Application.StockItems;
using ECommercePlatform.Modules.Order.Application.Abstractions;
using ECommercePlatform.Modules.Order.Application.Orders;
using ECommercePlatform.Modules.Order.Domain;
using MediatR;
using Moq;

namespace ECommercePlatform.Modules.Order.UnitTests;

public sealed class MarkOrderAsPaidCommandHandlerTests
{
    private readonly Mock<IOrderWriteRepository> _orderWriteRepository = new();
    private readonly Mock<ISender> _sender = new();
    private readonly MarkOrderAsPaidCommandHandler _handler;

    public MarkOrderAsPaidCommandHandlerTests()
    {
        _handler = new MarkOrderAsPaidCommandHandler(_orderWriteRepository.Object, _sender.Object);
    }

    [Fact]
    public async Task Handle_ShouldCommitReservedStock_WhenOrderIsMarkedPaid()
    {
        var variantId = Guid.NewGuid();
        var items = new List<OrderItemSnapshot> { new(variantId, "Telefon", "SKU-1", 100m, 3) };
        var order = Domain.Order.Create(
            Guid.NewGuid(), "Ayşe Yılmaz", "5551234567", "İstanbul", "Kadıköy", "Bir sokak No:1", "34000", items);
        order.MarkReadyForPayment();

        _orderWriteRepository
            .Setup(r => r.GetByIdWithItemsAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _handler.Handle(new MarkOrderAsPaidCommand(order.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Paid, order.Status);
        _sender.Verify(
            s => s.Send(
                It.Is<CommitStockCommand>(c => c.Items.Single().ProductVariantId == variantId && c.Items.Single().Quantity == 3),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
