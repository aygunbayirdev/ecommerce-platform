using ECommercePlatform.Modules.Shipping.Domain;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Shipping.UnitTests;

public sealed class ShipmentTests
{
    private static Domain.Shipment CreateShipment()
        => Domain.Shipment.Create(Guid.NewGuid(), "Yurtiçi Kargo", "YK123456789");

    [Fact]
    public void Create_ShouldStartAsShipped_WithFirstHistoryEntry()
    {
        var shipment = CreateShipment();

        Assert.Equal(ShipmentStatus.Shipped, shipment.Status);
        var history = Assert.Single(shipment.StatusHistory);
        Assert.Equal(ShipmentStatus.Shipped, history.Status);
    }

    [Fact]
    public void MarkInTransit_ShouldSucceed_FromShipped()
    {
        var shipment = CreateShipment();

        var result = shipment.MarkInTransit();

        Assert.True(result.IsSuccess);
        Assert.Equal(ShipmentStatus.InTransit, shipment.Status);
        Assert.Equal(2, shipment.StatusHistory.Count);
    }

    [Fact]
    public void MarkInTransit_ShouldReturnConflict_WhenCalledTwice()
    {
        var shipment = CreateShipment();
        shipment.MarkInTransit();

        var result = shipment.MarkInTransit();

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public void MarkDelivered_ShouldSucceed_FromShipped()
    {
        var shipment = CreateShipment();

        var result = shipment.MarkDelivered();

        Assert.True(result.IsSuccess);
        Assert.Equal(ShipmentStatus.Delivered, shipment.Status);
    }

    [Fact]
    public void MarkDelivered_ShouldSucceed_FromInTransit()
    {
        var shipment = CreateShipment();
        shipment.MarkInTransit();

        var result = shipment.MarkDelivered();

        Assert.True(result.IsSuccess);
        Assert.Equal(ShipmentStatus.Delivered, shipment.Status);
    }

    [Fact]
    public void MarkDelivered_ShouldReturnConflict_WhenAlreadyDelivered()
    {
        var shipment = CreateShipment();
        shipment.MarkDelivered();

        var result = shipment.MarkDelivered();

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public void MarkFailed_ShouldSucceed_FromShipped_AndStoreReason()
    {
        var shipment = CreateShipment();

        var result = shipment.MarkFailed("Paket kayboldu");

        Assert.True(result.IsSuccess);
        Assert.Equal(ShipmentStatus.Failed, shipment.Status);
        Assert.Equal("Paket kayboldu", shipment.FailureReason);
    }

    [Fact]
    public void MarkFailed_ShouldSucceed_FromInTransit()
    {
        var shipment = CreateShipment();
        shipment.MarkInTransit();

        var result = shipment.MarkFailed("Adreste bulunamadı");

        Assert.True(result.IsSuccess);
        Assert.Equal(ShipmentStatus.Failed, shipment.Status);
    }

    [Fact]
    public void MarkFailed_ShouldReturnConflict_WhenAlreadyDelivered()
    {
        var shipment = CreateShipment();
        shipment.MarkDelivered();

        var result = shipment.MarkFailed("Geç kaldı");

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }
}
