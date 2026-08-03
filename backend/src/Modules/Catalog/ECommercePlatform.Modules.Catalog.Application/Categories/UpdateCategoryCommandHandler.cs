using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Catalog.Application.Abstractions;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Catalog.Application.Categories;

public sealed class UpdateCategoryCommandHandler(ICategoryWriteRepository categoryWriteRepository)
    : ICommandHandler<UpdateCategoryCommand>
{
    public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await categoryWriteRepository.GetByIdAsync(request.CategoryId, cancellationToken);

        if (category is null)
        {
            return Result.Failure(Error.NotFound("Categories.NotFound", "Kategori bulunamadı."));
        }

        category.Rename(request.Name);
        category.ChangeDisplayOrder(request.DisplayOrder);

        await categoryWriteRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
