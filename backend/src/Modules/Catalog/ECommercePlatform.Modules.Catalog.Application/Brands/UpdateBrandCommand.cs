using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Catalog.Application.Brands;

public sealed record UpdateBrandCommand(Guid BrandId, string Name) : ICommand;
