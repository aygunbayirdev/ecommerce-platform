using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Catalog.Application.Products;

public sealed record ProductVariantAttributeValueInput(Guid ProductAttributeId, string Value);

public sealed record AddProductVariantCommand(
    Guid ProductId,
    string Sku,
    decimal Price,
    IReadOnlyList<ProductVariantAttributeValueInput> AttributeValues) : ICommand<Guid>;
