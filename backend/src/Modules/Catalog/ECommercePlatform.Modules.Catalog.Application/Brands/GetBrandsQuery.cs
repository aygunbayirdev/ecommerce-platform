using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Catalog.Application.Dtos;

namespace ECommercePlatform.Modules.Catalog.Application.Brands;

public sealed record GetBrandsQuery : IQuery<IReadOnlyList<BrandDto>>;
