using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Catalog.Application.Products;

public sealed record DeactivateProductVariantCommand(Guid ProductId, Guid ProductVariantId) : ICommand;
