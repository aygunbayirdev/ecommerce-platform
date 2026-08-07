using ECommercePlatform.Modules.Catalog.Domain;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Catalog.UnitTests;

public sealed class ProductImageTests
{
    private static Product CreateProduct() => Product.Create(Guid.NewGuid(), brandId: null, "Telefon", "Açıklama");

    [Fact]
    public void AddImage_FirstImage_BecomesPrimaryAutomatically()
    {
        var product = CreateProduct();

        var image = product.AddImage("https://example.com/1.jpg", isPrimary: false);

        Assert.True(image.IsPrimary);
    }

    [Fact]
    public void AddImage_WithIsPrimaryTrue_UnmarksExistingPrimary()
    {
        var product = CreateProduct();
        var first = product.AddImage("https://example.com/1.jpg", isPrimary: false);

        var second = product.AddImage("https://example.com/2.jpg", isPrimary: true);

        Assert.False(product.Images.Single(i => i.Id == first.Id).IsPrimary);
        Assert.True(second.IsPrimary);
    }

    [Fact]
    public void AddImage_WithoutIsPrimary_DoesNotChangeExistingPrimary()
    {
        var product = CreateProduct();
        var first = product.AddImage("https://example.com/1.jpg", isPrimary: false);

        var second = product.AddImage("https://example.com/2.jpg", isPrimary: false);

        Assert.True(product.Images.Single(i => i.Id == first.Id).IsPrimary);
        Assert.False(second.IsPrimary);
    }

    [Fact]
    public void RemoveImage_WhenPrimaryImageRemoved_AutoPromotesRemainingImageToPrimary()
    {
        var product = CreateProduct();
        var primary = product.AddImage("https://example.com/1.jpg", isPrimary: true);
        var second = product.AddImage("https://example.com/2.jpg", isPrimary: false);

        var result = product.RemoveImage(primary.Id);

        Assert.True(result.IsSuccess);
        Assert.True(product.Images.Single(i => i.Id == second.Id).IsPrimary);
    }

    [Fact]
    public void RemoveImage_WhenNonPrimaryImageRemoved_DoesNotChangeExistingPrimary()
    {
        var product = CreateProduct();
        var primary = product.AddImage("https://example.com/1.jpg", isPrimary: true);
        var second = product.AddImage("https://example.com/2.jpg", isPrimary: false);

        var result = product.RemoveImage(second.Id);

        Assert.True(result.IsSuccess);
        Assert.True(product.Images.Single(i => i.Id == primary.Id).IsPrimary);
    }

    [Fact]
    public void RemoveImage_WhenLastImageRemoved_LeavesEmptyImageListWithoutError()
    {
        var product = CreateProduct();
        var only = product.AddImage("https://example.com/1.jpg", isPrimary: false);

        var result = product.RemoveImage(only.Id);

        Assert.True(result.IsSuccess);
        Assert.Empty(product.Images);
    }

    [Fact]
    public void RemoveImage_ShouldReturnNotFound_WhenImageDoesNotExist()
    {
        var product = CreateProduct();
        product.AddImage("https://example.com/1.jpg", isPrimary: false);

        var result = product.RemoveImage(Guid.NewGuid());

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}
