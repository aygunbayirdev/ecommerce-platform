using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Catalog.Application.Categories;

public sealed record RemoveCategoryAttributeCommand(Guid CategoryId, Guid ProductAttributeId) : ICommand;
