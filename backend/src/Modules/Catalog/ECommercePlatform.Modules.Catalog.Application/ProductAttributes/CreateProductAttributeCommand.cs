using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Catalog.Application.ProductAttributes;

public sealed record CreateProductAttributeCommand(string Name) : ICommand<Guid>;
