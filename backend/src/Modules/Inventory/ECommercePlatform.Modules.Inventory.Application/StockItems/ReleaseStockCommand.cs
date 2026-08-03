using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Inventory.Application.Dtos;

namespace ECommercePlatform.Modules.Inventory.Application.StockItems;

public sealed record ReleaseStockCommand(IReadOnlyList<StockReservationItem> Items) : ICommand;
