using ECommercePlatform.Modules.Catalog.Domain;

namespace ECommercePlatform.Modules.Catalog.UnitTests;

public sealed class ProductVariantTests
{
    private static Product CreateProduct() => Product.Create(Guid.NewGuid(), brandId: null, "Telefon", "Açıklama");

    [Fact]
    public void AddVariant_ShouldSetAttributeValues()
    {
        var product = CreateProduct();
        var attributeId = Guid.NewGuid();

        var variant = product.AddVariant("SKU-1", 100m, new Dictionary<Guid, string> { [attributeId] = "Kırmızı" });

        var attributeValue = Assert.Single(variant.AttributeValues);
        Assert.Equal(attributeId, attributeValue.ProductAttributeId);
        Assert.Equal("Kırmızı", attributeValue.Value);
    }

    [Fact]
    public void AddVariant_ShouldAddToProductVariants()
    {
        var product = CreateProduct();

        var variant = product.AddVariant("SKU-1", 100m, new Dictionary<Guid, string>());

        Assert.Single(product.Variants);
        Assert.Equal(variant.Id, product.Variants.Single().Id);
    }
}
