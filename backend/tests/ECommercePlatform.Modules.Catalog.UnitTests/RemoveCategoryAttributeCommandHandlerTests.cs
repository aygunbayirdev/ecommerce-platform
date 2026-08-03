using ECommercePlatform.Modules.Catalog.Application.Abstractions;
using ECommercePlatform.Modules.Catalog.Application.Categories;
using ECommercePlatform.Modules.Catalog.Domain;
using ECommercePlatform.SharedKernel;
using Moq;

namespace ECommercePlatform.Modules.Catalog.UnitTests;

public sealed class RemoveCategoryAttributeCommandHandlerTests
{
    private readonly Mock<ICategoryWriteRepository> _categoryWriteRepository = new();
    private readonly Mock<IProductWriteRepository> _productWriteRepository = new();
    private readonly RemoveCategoryAttributeCommandHandler _handler;

    public RemoveCategoryAttributeCommandHandlerTests()
    {
        _handler = new RemoveCategoryAttributeCommandHandler(_categoryWriteRepository.Object, _productWriteRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnConflict_WhenAttributeIsUsedByAVariantInCategory()
    {
        var category = Category.Create("Elektronik", parentCategoryId: null, displayOrder: 0);
        var attributeId = Guid.NewGuid();
        category.AssignAttribute(attributeId);
        var command = new RemoveCategoryAttributeCommand(category.Id, attributeId);

        _categoryWriteRepository
            .Setup(r => r.GetByIdWithAttributesAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _productWriteRepository
            .Setup(r => r.IsAttributeUsedByAnyVariantInCategoryAsync(category.Id, attributeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Single(category.Attributes);
        _categoryWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldRemoveAttribute_WhenNotUsedByAnyVariant()
    {
        var category = Category.Create("Elektronik", parentCategoryId: null, displayOrder: 0);
        var attributeId = Guid.NewGuid();
        category.AssignAttribute(attributeId);
        var command = new RemoveCategoryAttributeCommand(category.Id, attributeId);

        _categoryWriteRepository
            .Setup(r => r.GetByIdWithAttributesAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _productWriteRepository
            .Setup(r => r.IsAttributeUsedByAnyVariantInCategoryAsync(category.Id, attributeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(category.Attributes);
        _categoryWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
