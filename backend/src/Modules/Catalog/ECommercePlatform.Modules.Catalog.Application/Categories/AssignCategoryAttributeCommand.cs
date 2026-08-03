using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Catalog.Application.Categories;

public sealed record AssignCategoryAttributeCommand(Guid CategoryId, Guid ProductAttributeId) : ICommand;
