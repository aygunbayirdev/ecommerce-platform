using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Catalog.Application.Abstractions;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Catalog.Application.Categories;

public sealed class RemoveCategoryAttributeCommandHandler(ICategoryWriteRepository categoryWriteRepository)
    : ICommandHandler<RemoveCategoryAttributeCommand>
{
    public async Task<Result> Handle(RemoveCategoryAttributeCommand request, CancellationToken cancellationToken)
    {
        var category = await categoryWriteRepository.GetByIdWithAttributesAsync(request.CategoryId, cancellationToken);

        if (category is null)
        {
            return Result.Failure(Error.NotFound("Categories.NotFound", "Kategori bulunamadı."));
        }

        var result = category.RemoveAttribute(request.ProductAttributeId);

        if (result.IsFailure)
        {
            return result;
        }

        await categoryWriteRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
