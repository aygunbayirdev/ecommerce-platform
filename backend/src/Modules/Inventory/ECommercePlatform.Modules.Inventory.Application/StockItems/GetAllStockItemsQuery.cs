using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.BuildingBlocks.Application.Models;
using ECommercePlatform.Modules.Inventory.Application.Dtos;

namespace ECommercePlatform.Modules.Inventory.Application.StockItems;

public sealed record GetAllStockItemsQuery(int PageNumber, int PageSize)
    : IQuery<PagedResult<StockItemDto>>;
