using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Inventory.Application.StockItems;

public sealed record CreateStockItemCommand(Guid ProductVariantId) : ICommand<Guid>;
