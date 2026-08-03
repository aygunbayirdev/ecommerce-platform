using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.BuildingBlocks.Application.Models;
using ECommercePlatform.Modules.Catalog.Application.Dtos;

namespace ECommercePlatform.Modules.Catalog.Application.Products;

public sealed record GetProductsByCategoryQuery(Guid CategoryId, int PageNumber, int PageSize)
    : IQuery<PagedResult<ProductSummaryDto>>;
