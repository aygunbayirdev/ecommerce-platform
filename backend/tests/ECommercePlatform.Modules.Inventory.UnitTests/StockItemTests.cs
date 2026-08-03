using ECommercePlatform.Modules.Inventory.Domain;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Inventory.UnitTests;

public sealed class StockItemTests
{
    [Fact]
    public void Create_ShouldStartWithZeroQuantities()
    {
        var stockItem = StockItem.Create(Guid.NewGuid());

        Assert.Equal(0, stockItem.AvailableQuantity);
        Assert.Equal(0, stockItem.ReservedQuantity);
    }

    [Fact]
    public void IncreaseStock_ShouldIncreaseAvailableQuantityAndAddMovement()
    {
        var stockItem = StockItem.Create(Guid.NewGuid());

        var result = stockItem.IncreaseStock(10, "İlk sevkiyat");

        Assert.True(result.IsSuccess);
        Assert.Equal(10, stockItem.AvailableQuantity);
        var movement = Assert.Single(stockItem.Movements);
        Assert.Equal(StockMovementType.Inbound, movement.MovementType);
        Assert.Equal(10, movement.Quantity);
    }

    [Fact]
    public void IncreaseStock_ShouldAccumulateAcrossMultipleCalls()
    {
        var stockItem = StockItem.Create(Guid.NewGuid());

        stockItem.IncreaseStock(10, null);
        stockItem.IncreaseStock(5, null);

        Assert.Equal(15, stockItem.AvailableQuantity);
        Assert.Equal(2, stockItem.Movements.Count);
    }

    [Fact]
    public void IncreaseStock_ShouldReturnValidationError_WhenQuantityIsNotPositive()
    {
        var stockItem = StockItem.Create(Guid.NewGuid());

        var result = stockItem.IncreaseStock(0, null);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(0, stockItem.AvailableQuantity);
    }
}
