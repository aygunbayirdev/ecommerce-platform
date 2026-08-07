using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Catalog.Application.Products;

public sealed record DeactivateProductCommand(Guid ProductId) : ICommand;
