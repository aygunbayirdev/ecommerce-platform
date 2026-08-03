using ECommercePlatform.Modules.Catalog.Domain;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Catalog.UnitTests;

public sealed class CategoryAttributeTests
{
    [Fact]
    public void AssignAttribute_ShouldSucceed_WhenNotAlreadyAssigned()
    {
        var category = Category.Create("Giyim", parentCategoryId: null, displayOrder: 0);
        var attributeId = Guid.NewGuid();

        var result = category.AssignAttribute(attributeId);

        Assert.True(result.IsSuccess);
        Assert.Single(category.Attributes);
    }

    [Fact]
    public void AssignAttribute_ShouldReturnConflict_WhenAlreadyAssigned()
    {
        var category = Category.Create("Giyim", parentCategoryId: null, displayOrder: 0);
        var attributeId = Guid.NewGuid();
        category.AssignAttribute(attributeId);

        var result = category.AssignAttribute(attributeId);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Single(category.Attributes);
    }

    [Fact]
    public void RemoveAttribute_ShouldSucceed_WhenAssigned()
    {
        var category = Category.Create("Giyim", parentCategoryId: null, displayOrder: 0);
        var attributeId = Guid.NewGuid();
        category.AssignAttribute(attributeId);

        var result = category.RemoveAttribute(attributeId);

        Assert.True(result.IsSuccess);
        Assert.Empty(category.Attributes);
    }

    [Fact]
    public void RemoveAttribute_ShouldReturnNotFound_WhenNotAssigned()
    {
        var category = Category.Create("Giyim", parentCategoryId: null, displayOrder: 0);

        var result = category.RemoveAttribute(Guid.NewGuid());

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}
