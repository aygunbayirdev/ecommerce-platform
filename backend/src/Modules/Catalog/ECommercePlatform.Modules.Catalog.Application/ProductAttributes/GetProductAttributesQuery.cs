using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Catalog.Application.Dtos;

namespace ECommercePlatform.Modules.Catalog.Application.ProductAttributes;

public sealed record GetProductAttributesQuery : IQuery<IReadOnlyList<ProductAttributeDto>>;
