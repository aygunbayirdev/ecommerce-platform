using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Catalog.Application.Products;

public sealed record ReactivateProductVariantCommand(Guid ProductId, Guid ProductVariantId) : ICommand;
