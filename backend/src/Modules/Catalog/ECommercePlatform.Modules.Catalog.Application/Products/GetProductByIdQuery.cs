using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Catalog.Application.Dtos;

namespace ECommercePlatform.Modules.Catalog.Application.Products;

public sealed record GetProductByIdQuery(Guid ProductId) : IQuery<ProductDetailDto>;
