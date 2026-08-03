using ECommercePlatform.Modules.Inventory.Application.Abstractions;
using ECommercePlatform.Modules.Inventory.Application.StockItems;
using ECommercePlatform.Modules.Inventory.Domain;
using Moq;

namespace ECommercePlatform.Modules.Inventory.UnitTests;

public sealed class CreateStockItemCommandHandlerTests
{
    private readonly Mock<IStockItemWriteRepository> _stockItemWriteRepository = new();
    private readonly CreateStockItemCommandHandler _handler;

    public CreateStockItemCommandHandlerTests()
    {
        _handler = new CreateStockItemCommandHandler(_stockItemWriteRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateStockItem_WhenNoneExistsForVariant()
    {
        var productVariantId = Guid.NewGuid();
        var command = new CreateStockItemCommand(productVariantId);

        _stockItemWriteRepository
            .Setup(r => r.GetByProductVariantIdAsync(productVariantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StockItem?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _stockItemWriteRepository.Verify(r => r.Add(It.IsAny<StockItem>()), Times.Once);
        _stockItemWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnExistingId_WhenStockItemAlreadyExists_Idempotent()
    {
        var productVariantId = Guid.NewGuid();
        var existing = StockItem.Create(productVariantId);
        var command = new CreateStockItemCommand(productVariantId);

        _stockItemWriteRepository
            .Setup(r => r.GetByProductVariantIdAsync(productVariantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(existing.Id, result.Value);
        _stockItemWriteRepository.Verify(r => r.Add(It.IsAny<StockItem>()), Times.Never);
        _stockItemWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
