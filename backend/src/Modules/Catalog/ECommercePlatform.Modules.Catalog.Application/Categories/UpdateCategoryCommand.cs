using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Catalog.Application.Categories;

public sealed record UpdateCategoryCommand(Guid CategoryId, string Name, int DisplayOrder) : ICommand;
