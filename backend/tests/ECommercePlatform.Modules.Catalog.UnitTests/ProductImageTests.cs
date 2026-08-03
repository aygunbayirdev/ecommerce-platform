using ECommercePlatform.Modules.Catalog.Domain;

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
}
