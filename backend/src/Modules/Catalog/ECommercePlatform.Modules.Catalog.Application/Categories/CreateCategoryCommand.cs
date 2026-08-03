using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Catalog.Application.Categories;

public sealed record CreateCategoryCommand(string Name, Guid? ParentCategoryId, int DisplayOrder) : ICommand<Guid>;
