using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Catalog.Application.Dtos;

namespace ECommercePlatform.Modules.Catalog.Application.Products;

public sealed record GetProductVariantSummariesQuery(IReadOnlyList<Guid> ProductVariantIds)
    : IQuery<IReadOnlyList<ProductVariantSummaryDto>>;
