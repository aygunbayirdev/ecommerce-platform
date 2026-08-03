using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Catalog.Domain;

public sealed class ProductVariantAttributeValue : BaseEntity
{
    private ProductVariantAttributeValue()
    {
    }

    public Guid ProductVariantId { get; private set; }

    public Guid ProductAttributeId { get; private set; }

    public string Value { get; private set; } = string.Empty;

    internal static ProductVariantAttributeValue Create(Guid productVariantId, Guid productAttributeId, string value)
    {
        return new ProductVariantAttributeValue
        {
            ProductVariantId = Guard.AgainstEmpty(productVariantId, nameof(productVariantId)),
            ProductAttributeId = Guard.AgainstEmpty(productAttributeId, nameof(productAttributeId)),
            Value = Guard.AgainstNullOrWhiteSpace(value, nameof(value)),
        };
    }

    internal void UpdateValue(string value)
    {
        Value = Guard.AgainstNullOrWhiteSpace(value, nameof(value));
    }
}
