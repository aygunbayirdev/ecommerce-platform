using ECommercePlatform.Modules.Inventory.Application.Abstractions;
using ECommercePlatform.Modules.Inventory.Application.Dtos;
using ECommercePlatform.Modules.Inventory.Application.StockItems;
using ECommercePlatform.Modules.Inventory.Domain;
using ECommercePlatform.SharedKernel;
using Moq;

namespace ECommercePlatform.Modules.Inventory.UnitTests;

public sealed class ReserveStockCommandHandlerTests
{
    private readonly Mock<IStockItemWriteRepository> _stockItemWriteRepository = new();
    private readonly ReserveStockCommandHandler _handler;

    public ReserveStockCommandHandlerTests()
    {
        _handler = new ReserveStockCommandHandler(_stockItemWriteRepository.Object);
    }

    private static StockItem CreateStockItemWithAvailable(int available)
    {
        var stockItem = StockItem.Create(Guid.NewGuid());
        stockItem.IncreaseStock(available, reason: null);
        return stockItem;
    }

    [Fact]
    public async Task Handle_ShouldReserveAllItems_OnHappyPath()
    {
        var stockItemA = CreateStockItemWithAvailable(10);
        var stockItemB = CreateStockItemWithAvailable(5);
        var command = new ReserveStockCommand(
        [
            new StockReservationItem(stockItemA.ProductVariantId, 3),
            new StockReservationItem(stockItemB.ProductVariantId, 2),
        ]);

        _stockItemWriteRepository
            .Setup(r => r.GetByProductVariantIdsAsync(
                It.Is<IReadOnlyList<Guid>>(ids => ids.Contains(stockItemA.ProductVariantId) && ids.Contains(stockItemB.ProductVariantId)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([stockItemA, stockItemB]);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(7, stockItemA.AvailableQuantity);
        Assert.Equal(3, stockItemA.ReservedQuantity);
        Assert.Equal(3, stockItemB.AvailableQuantity);
        Assert.Equal(2, stockItemB.ReservedQuantity);
        _stockItemWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFoundWithoutSaving_WhenAVariantHasNoStockRecord()
    {
        var stockItemA = CreateStockItemWithAvailable(10);
        var missingVariantId = Guid.NewGuid();
        var command = new ReserveStockCommand(
        [
            new StockReservationItem(stockItemA.ProductVariantId, 3),
            new StockReservationItem(missingVariantId, 1),
        ]);

        _stockItemWriteRepository
            .Setup(r => r.GetByProductVariantIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([stockItemA]);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        _stockItemWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnConflictWithoutSaving_WhenAnyItemHasInsufficientStock()
    {
        var stockItemA = CreateStockItemWithAvailable(10);
        var stockItemB = CreateStockItemWithAvailable(1);
        var command = new ReserveStockCommand(
        [
            new StockReservationItem(stockItemA.ProductVariantId, 3),
            new StockReservationItem(stockItemB.ProductVariantId, 5),
        ]);

        _stockItemWriteRepository
            .Setup(r => r.GetByProductVariantIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([stockItemA, stockItemB]);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        _stockItemWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
