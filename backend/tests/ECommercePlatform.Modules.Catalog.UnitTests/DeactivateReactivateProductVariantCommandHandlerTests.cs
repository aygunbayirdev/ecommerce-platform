using ECommercePlatform.Modules.Catalog.Application.Abstractions;
using ECommercePlatform.Modules.Catalog.Application.Products;
using ECommercePlatform.Modules.Catalog.Domain;
using ECommercePlatform.SharedKernel;
using Moq;

namespace ECommercePlatform.Modules.Catalog.UnitTests;

public sealed class DeactivateReactivateProductVariantCommandHandlerTests
{
    private readonly Mock<IProductWriteRepository> _productWriteRepository = new();

    private static Product CreateProductWithVariant(out ProductVariant variant)
    {
        var product = Product.Create(Guid.NewGuid(), brandId: null, "Telefon", "Açıklama");
        variant = product.AddVariant("SKU-1", 100m, new Dictionary<Guid, string>());

        return product;
    }

    [Fact]
    public async Task Deactivate_ShouldSucceed_WhenVariantIsActive()
    {
        var product = CreateProductWithVariant(out var variant);
        var handler = new DeactivateProductVariantCommandHandler(_productWriteRepository.Object);
        _productWriteRepository
            .Setup(r => r.GetByIdWithVariantsAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await handler.Handle(
            new DeactivateProductVariantCommand(product.Id, variant.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(variant.IsActive);
        _productWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Deactivate_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        var handler = new DeactivateProductVariantCommandHandler(_productWriteRepository.Object);
        var productId = Guid.NewGuid();
        _productWriteRepository
            .Setup(r => r.GetByIdWithVariantsAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var result = await handler.Handle(
            new DeactivateProductVariantCommand(productId, Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Deactivate_ShouldReturnNotFound_WhenVariantDoesNotExistOnProduct()
    {
        var product = CreateProductWithVariant(out _);
        var handler = new DeactivateProductVariantCommandHandler(_productWriteRepository.Object);
        _productWriteRepository
            .Setup(r => r.GetByIdWithVariantsAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await handler.Handle(
            new DeactivateProductVariantCommand(product.Id, Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Deactivate_ShouldReturnConflict_WhenVariantAlreadyInactive()
    {
        var product = CreateProductWithVariant(out var variant);
        product.DeactivateVariant(variant.Id);
        var handler = new DeactivateProductVariantCommandHandler(_productWriteRepository.Object);
        _productWriteRepository
            .Setup(r => r.GetByIdWithVariantsAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await handler.Handle(
            new DeactivateProductVariantCommand(product.Id, variant.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public async Task Reactivate_ShouldSucceed_WhenVariantIsInactive()
    {
        var product = CreateProductWithVariant(out var variant);
        product.DeactivateVariant(variant.Id);
        var handler = new ReactivateProductVariantCommandHandler(_productWriteRepository.Object);
        _productWriteRepository
            .Setup(r => r.GetByIdWithVariantsAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await handler.Handle(
            new ReactivateProductVariantCommand(product.Id, variant.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(variant.IsActive);
        _productWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Reactivate_ShouldReturnConflict_WhenVariantAlreadyActive()
    {
        var product = CreateProductWithVariant(out var variant);
        var handler = new ReactivateProductVariantCommandHandler(_productWriteRepository.Object);
        _productWriteRepository
            .Setup(r => r.GetByIdWithVariantsAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await handler.Handle(
            new ReactivateProductVariantCommand(product.Id, variant.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }
}
