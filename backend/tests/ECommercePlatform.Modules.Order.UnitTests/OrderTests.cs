using ECommercePlatform.Modules.Order.Domain;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Order.UnitTests;

public sealed class OrderTests
{
    private static Domain.Order CreateOrder()
    {
        var items = new List<OrderItemSnapshot>
        {
            new(Guid.NewGuid(), "Telefon", "SKU-1", 100m, 2),
            new(Guid.NewGuid(), "Kılıf", "SKU-2", 25m, 1),
        };

        return Domain.Order.Create(
            Guid.NewGuid(), "Ayşe Yılmaz", "5551234567", "İstanbul", "Kadıköy", "Bir sokak No:1", "34000", items);
    }

    [Fact]
    public void Create_ShouldGenerateDistinctOrderNumbers_ForOrdersCreatedInQuickSuccession()
    {
        // Regression test: OrderNumber used to be derived from the FIRST 8 hex chars of the
        // Guid, which are a UUIDv7 timestamp — orders created close together collided on the
        // order_number unique index. It must be derived from the random trailing bits instead.
        var orderNumbers = Enumerable.Range(0, 100).Select(_ => CreateOrder().OrderNumber).ToList();

        Assert.Equal(orderNumbers.Count, orderNumbers.Distinct().Count());
    }

    [Fact]
    public void Create_ShouldStartAsCreated_WithFirstHistoryEntry()
    {
        var order = CreateOrder();

        Assert.Equal(OrderStatus.Created, order.Status);
        var history = Assert.Single(order.StatusHistory);
        Assert.Equal(OrderStatus.Created, history.Status);
    }

    [Fact]
    public void Create_ShouldComputeTotalFromItemLineTotals()
    {
        var order = CreateOrder();

        Assert.Equal(225m, order.Total);
    }

    [Fact]
    public void ApplyDiscount_ShouldReduceTotal()
    {
        var order = CreateOrder();

        var result = order.ApplyDiscount("SAVE10", 22.5m);

        Assert.True(result.IsSuccess);
        Assert.Equal("SAVE10", order.CouponCode);
        Assert.Equal(202.5m, order.Total);
    }

    [Fact]
    public void ApplyDiscount_ShouldReturnConflict_WhenCalledTwice()
    {
        var order = CreateOrder();
        order.ApplyDiscount("SAVE10", 22.5m);

        var result = order.ApplyDiscount("SAVE20", 45m);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal("SAVE10", order.CouponCode);
    }

    [Fact]
    public void Total_ShouldNeverGoBelowZero_WhenDiscountExceedsSubtotal()
    {
        var order = CreateOrder();

        order.ApplyDiscount("HUGE", 1000m);

        Assert.Equal(0m, order.Total);
    }

    [Fact]
    public void MarkReadyForPayment_ShouldTransitionFromCreated()
    {
        var order = CreateOrder();

        var result = order.MarkReadyForPayment();

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.PaymentPending, order.Status);
        Assert.Equal(2, order.StatusHistory.Count);
    }

    [Fact]
    public void MarkReadyForPayment_ShouldReturnConflict_WhenCalledTwice()
    {
        var order = CreateOrder();
        order.MarkReadyForPayment();

        var result = order.MarkReadyForPayment();

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public void Cancel_ShouldSucceed_WhenCreated()
    {
        var order = CreateOrder();

        var result = order.Cancel("Müşteri vazgeçti");

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Equal("Müşteri vazgeçti", order.StatusHistory.Last().Note);
    }

    [Fact]
    public void Cancel_ShouldSucceed_WhenPaymentPending()
    {
        var order = CreateOrder();
        order.MarkReadyForPayment();

        var result = order.Cancel("Yetersiz stok");

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void Cancel_ShouldReturnConflict_WhenShipped()
    {
        var order = CreateOrder();
        order.MarkReadyForPayment();
        order.MarkAsPaid();
        order.MarkAsPreparing();
        order.MarkAsShipped();

        var result = order.Cancel("Çok geç");

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(OrderStatus.Shipped, order.Status);
    }

    [Fact]
    public void FullLifecycle_ShouldTransitionThroughAllStatusesInOrder()
    {
        var order = CreateOrder();

        Assert.True(order.MarkReadyForPayment().IsSuccess);
        Assert.True(order.MarkAsPaid().IsSuccess);
        Assert.True(order.MarkAsPreparing().IsSuccess);
        Assert.True(order.MarkAsShipped().IsSuccess);
        Assert.True(order.MarkAsDelivered().IsSuccess);

        Assert.Equal(OrderStatus.Delivered, order.Status);
        Assert.Equal(6, order.StatusHistory.Count);
    }

    [Fact]
    public void MarkAsShipped_ShouldReturnConflict_WhenCalledOutOfOrder()
    {
        var order = CreateOrder();

        var result = order.MarkAsShipped();

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(OrderStatus.Created, order.Status);
    }
}
