using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Catalog.Application.Abstractions;
using ECommercePlatform.Modules.Catalog.Domain;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Catalog.Application.Categories;

public sealed class CreateCategoryCommandHandler(ICategoryWriteRepository categoryWriteRepository)
    : ICommandHandler<CreateCategoryCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (request.ParentCategoryId is { } parentId)
        {
            var parent = await categoryWriteRepository.GetByIdAsync(parentId, cancellationToken);

            if (parent is null)
            {
                return Result.Failure<Guid>(Error.NotFound("Categories.ParentNotFound", "Üst kategori bulunamadı."));
            }
        }

        var category = Category.Create(request.Name, request.ParentCategoryId, request.DisplayOrder);

        categoryWriteRepository.Add(category);
        await categoryWriteRepository.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}
