using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Inventory.Application.StockItems;

public sealed record IncreaseStockCommand(Guid ProductVariantId, int Quantity, string? Reason) : ICommand;
