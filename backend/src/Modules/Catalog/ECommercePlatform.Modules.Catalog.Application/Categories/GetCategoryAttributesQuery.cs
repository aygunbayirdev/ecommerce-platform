using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Catalog.Application.Dtos;

namespace ECommercePlatform.Modules.Catalog.Application.Categories;

public sealed record GetCategoryAttributesQuery(Guid CategoryId) : IQuery<IReadOnlyList<ProductAttributeDto>>;
