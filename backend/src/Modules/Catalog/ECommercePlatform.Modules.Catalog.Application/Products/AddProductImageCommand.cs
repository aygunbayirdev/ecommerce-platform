using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Catalog.Application.Products;

public sealed record AddProductImageCommand(Guid ProductId, string Url, bool IsPrimary) : ICommand<Guid>;
