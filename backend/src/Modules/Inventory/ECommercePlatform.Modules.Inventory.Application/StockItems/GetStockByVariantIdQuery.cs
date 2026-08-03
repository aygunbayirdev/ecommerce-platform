using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Inventory.Application.Dtos;

namespace ECommercePlatform.Modules.Inventory.Application.StockItems;

public sealed record GetStockByVariantIdQuery(Guid ProductVariantId) : IQuery<StockItemDto>;
