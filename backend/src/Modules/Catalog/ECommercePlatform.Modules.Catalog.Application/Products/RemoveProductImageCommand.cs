using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Catalog.Application.Products;

public sealed record RemoveProductImageCommand(Guid ProductId, Guid ProductImageId) : ICommand;
