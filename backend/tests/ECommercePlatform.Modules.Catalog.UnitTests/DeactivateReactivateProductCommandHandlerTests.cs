using ECommercePlatform.Modules.Catalog.Application.Abstractions;
using ECommercePlatform.Modules.Catalog.Application.Products;
using ECommercePlatform.Modules.Catalog.Domain;
using ECommercePlatform.SharedKernel;
using Moq;

namespace ECommercePlatform.Modules.Catalog.UnitTests;

public sealed class DeactivateReactivateProductCommandHandlerTests
{
    private readonly Mock<IProductWriteRepository> _productWriteRepository = new();

    private static Product CreateProduct() => Product.Create(Guid.NewGuid(), brandId: null, "Telefon", "Açıklama");

    [Fact]
    public async Task Deactivate_ShouldSucceed_WhenProductIsActive()
    {
        var product = CreateProduct();
        var handler = new DeactivateProductCommandHandler(_productWriteRepository.Object);
        _productWriteRepository.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var result = await handler.Handle(new DeactivateProductCommand(product.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(product.IsActive);
        _productWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Deactivate_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        var handler = new DeactivateProductCommandHandler(_productWriteRepository.Object);
        var productId = Guid.NewGuid();
        _productWriteRepository.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        var result = await handler.Handle(new DeactivateProductCommand(productId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Deactivate_ShouldReturnConflict_WhenProductAlreadyInactive()
    {
        var product = CreateProduct();
        product.Deactivate();
        var handler = new DeactivateProductCommandHandler(_productWriteRepository.Object);
        _productWriteRepository.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var result = await handler.Handle(new DeactivateProductCommand(product.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        _productWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Reactivate_ShouldSucceed_WhenProductIsInactive()
    {
        var product = CreateProduct();
        product.Deactivate();
        var handler = new ReactivateProductCommandHandler(_productWriteRepository.Object);
        _productWriteRepository.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var result = await handler.Handle(new ReactivateProductCommand(product.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(product.IsActive);
        _productWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Reactivate_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        var handler = new ReactivateProductCommandHandler(_productWriteRepository.Object);
        var productId = Guid.NewGuid();
        _productWriteRepository.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        var result = await handler.Handle(new ReactivateProductCommand(productId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Reactivate_ShouldReturnConflict_WhenProductAlreadyActive()
    {
        var product = CreateProduct();
        var handler = new ReactivateProductCommandHandler(_productWriteRepository.Object);
        _productWriteRepository.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var result = await handler.Handle(new ReactivateProductCommand(product.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }
}
