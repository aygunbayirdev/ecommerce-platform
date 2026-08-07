using ECommercePlatform.Modules.Inventory.Application.Abstractions;
using ECommercePlatform.Modules.Inventory.Application.Dtos;
using ECommercePlatform.Modules.Inventory.Application.StockItems;
using ECommercePlatform.Modules.Inventory.Domain;
using ECommercePlatform.SharedKernel;
using Moq;

namespace ECommercePlatform.Modules.Inventory.UnitTests;

public sealed class CommitStockCommandHandlerTests
{
    private readonly Mock<IStockItemWriteRepository> _stockItemWriteRepository = new();
    private readonly CommitStockCommandHandler _handler;

    public CommitStockCommandHandlerTests()
    {
        _handler = new CommitStockCommandHandler(_stockItemWriteRepository.Object);
    }

    private static StockItem CreateStockItemWithReserved(int reserved)
    {
        var stockItem = StockItem.Create(Guid.NewGuid());
        stockItem.IncreaseStock(reserved, reason: null);
        stockItem.Reserve(reserved);
        return stockItem;
    }

    [Fact]
    public async Task Handle_ShouldCommitAllItems_OnHappyPath()
    {
        var stockItemA = CreateStockItemWithReserved(3);
        var stockItemB = CreateStockItemWithReserved(2);
        var command = new CommitStockCommand(
        [
            new StockReservationItem(stockItemA.ProductVariantId, 3),
            new StockReservationItem(stockItemB.ProductVariantId, 2),
        ]);

        _stockItemWriteRepository
            .Setup(r => r.GetByProductVariantIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([stockItemA, stockItemB]);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, stockItemA.ReservedQuantity);
        Assert.Equal(0, stockItemB.ReservedQuantity);
        _stockItemWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFoundWithoutSaving_WhenAVariantHasNoStockRecord()
    {
        var stockItemA = CreateStockItemWithReserved(3);
        var missingVariantId = Guid.NewGuid();
        var command = new CommitStockCommand(
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
    public async Task Handle_ShouldReturnConflictWithoutSaving_WhenCommittingMoreThanReserved()
    {
        var stockItemA = CreateStockItemWithReserved(3);
        var stockItemB = CreateStockItemWithReserved(1);
        var command = new CommitStockCommand(
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
