using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Catalog.Application.Products;

public sealed record CreateProductCommand(Guid CategoryId, Guid? BrandId, string Name, string Description) : ICommand<Guid>;
