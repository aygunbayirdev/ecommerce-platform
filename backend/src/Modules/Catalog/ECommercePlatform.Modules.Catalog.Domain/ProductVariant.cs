using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Catalog.Domain;

public sealed class ProductVariant : BaseEntity
{
    private readonly List<ProductVariantAttributeValue> _attributeValues = [];

    private ProductVariant()
    {
    }

    public Guid ProductId { get; private set; }

    public string Sku { get; private set; } = string.Empty;

    public decimal Price { get; private set; }

    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<ProductVariantAttributeValue> AttributeValues => _attributeValues.AsReadOnly();

    internal static ProductVariant Create(Guid productId, string sku, decimal price)
    {
        var variant = new ProductVariant
        {
            ProductId = Guard.AgainstEmpty(productId, nameof(productId)),
            Sku = Guard.AgainstNullOrWhiteSpace(sku, nameof(sku)),
            Price = price,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
        };

        variant.AddDomainEvent(new ProductVariantCreatedDomainEvent(variant.Id, variant.ProductId));

        return variant;
    }

    internal void Deactivate() => IsActive = false;

    internal void Reactivate() => IsActive = true;

    internal void SetAttributeValue(Guid productAttributeId, string value)
    {
        var existing = _attributeValues.FirstOrDefault(v => v.ProductAttributeId == productAttributeId);

        if (existing is not null)
        {
            existing.UpdateValue(value);
            return;
        }

        _attributeValues.Add(ProductVariantAttributeValue.Create(Id, productAttributeId, value));
    }
}
