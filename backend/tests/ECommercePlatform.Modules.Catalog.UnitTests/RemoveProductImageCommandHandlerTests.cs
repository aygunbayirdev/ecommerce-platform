using ECommercePlatform.Modules.Catalog.Application.Abstractions;
using ECommercePlatform.Modules.Catalog.Application.Products;
using ECommercePlatform.Modules.Catalog.Domain;
using ECommercePlatform.SharedKernel;
using Moq;

namespace ECommercePlatform.Modules.Catalog.UnitTests;

public sealed class RemoveProductImageCommandHandlerTests
{
    private readonly Mock<IProductWriteRepository> _productWriteRepository = new();

    private static Product CreateProductWithImage(out ProductImage image)
    {
        var product = Product.Create(Guid.NewGuid(), brandId: null, "Telefon", "Açıklama");
        image = product.AddImage("https://example.com/1.jpg", isPrimary: false);

        return product;
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenImageExists()
    {
        var product = CreateProductWithImage(out var image);
        var handler = new RemoveProductImageCommandHandler(_productWriteRepository.Object);
        _productWriteRepository
            .Setup(r => r.GetByIdWithVariantsAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await handler.Handle(new RemoveProductImageCommand(product.Id, image.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(product.Images);
        _productWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        var handler = new RemoveProductImageCommandHandler(_productWriteRepository.Object);
        var productId = Guid.NewGuid();
        _productWriteRepository
            .Setup(r => r.GetByIdWithVariantsAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var result = await handler.Handle(new RemoveProductImageCommand(productId, Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenImageDoesNotExistOnProduct()
    {
        var product = CreateProductWithImage(out _);
        var handler = new RemoveProductImageCommandHandler(_productWriteRepository.Object);
        _productWriteRepository
            .Setup(r => r.GetByIdWithVariantsAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await handler.Handle(new RemoveProductImageCommand(product.Id, Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        _productWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
