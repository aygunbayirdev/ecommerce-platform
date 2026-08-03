using ECommercePlatform.Modules.Catalog.Application.Abstractions;
using ECommercePlatform.Modules.Catalog.Application.Products;
using ECommercePlatform.Modules.Catalog.Domain;
using ECommercePlatform.SharedKernel;
using Moq;

namespace ECommercePlatform.Modules.Catalog.UnitTests;

public sealed class AddProductVariantCommandHandlerTests
{
    private readonly Mock<IProductWriteRepository> _productWriteRepository = new();
    private readonly Mock<ICategoryWriteRepository> _categoryWriteRepository = new();
    private readonly AddProductVariantCommandHandler _handler;

    public AddProductVariantCommandHandlerTests()
    {
        _handler = new AddProductVariantCommandHandler(_productWriteRepository.Object, _categoryWriteRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldAddVariant_WhenAttributeIsAssignedToCategory()
    {
        var category = Category.Create("Elektronik", parentCategoryId: null, displayOrder: 0);
        var attributeId = Guid.NewGuid();
        category.AssignAttribute(attributeId);

        var product = Product.Create(category.Id, brandId: null, "Telefon", "Açıklama");
        var command = new AddProductVariantCommand(
            product.Id, "SKU-1", 100m, [new ProductVariantAttributeValueInput(attributeId, "128GB")]);

        _productWriteRepository
            .Setup(r => r.GetByIdWithVariantsAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _productWriteRepository.Setup(r => r.ExistsBySkuAsync("SKU-1", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _categoryWriteRepository
            .Setup(r => r.GetByIdWithAttributesAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(product.Variants);
        _productWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationError_WhenAttributeNotAssignedToCategory()
    {
        var category = Category.Create("Elektronik", parentCategoryId: null, displayOrder: 0);
        var product = Product.Create(category.Id, brandId: null, "Telefon", "Açıklama");
        var unassignedAttributeId = Guid.NewGuid();
        var command = new AddProductVariantCommand(
            product.Id, "SKU-1", 100m, [new ProductVariantAttributeValueInput(unassignedAttributeId, "128GB")]);

        _productWriteRepository
            .Setup(r => r.GetByIdWithVariantsAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _productWriteRepository.Setup(r => r.ExistsBySkuAsync("SKU-1", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _categoryWriteRepository
            .Setup(r => r.GetByIdWithAttributesAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Empty(product.Variants);
    }

    [Fact]
    public async Task Handle_ShouldReturnConflict_WhenSkuAlreadyExists()
    {
        var product = Product.Create(Guid.NewGuid(), brandId: null, "Telefon", "Açıklama");
        var command = new AddProductVariantCommand(product.Id, "SKU-1", 100m, []);

        _productWriteRepository
            .Setup(r => r.GetByIdWithVariantsAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _productWriteRepository.Setup(r => r.ExistsBySkuAsync("SKU-1", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }
}
