using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Catalog.Application.Brands;

public sealed record CreateBrandCommand(string Name) : ICommand<Guid>;
