using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Catalog.Application.Abstractions;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Catalog.Application.Categories;

public sealed class AssignCategoryAttributeCommandHandler(
    ICategoryWriteRepository categoryWriteRepository,
    IProductAttributeWriteRepository productAttributeWriteRepository) : ICommandHandler<AssignCategoryAttributeCommand>
{
    public async Task<Result> Handle(AssignCategoryAttributeCommand request, CancellationToken cancellationToken)
    {
        var category = await categoryWriteRepository.GetByIdWithAttributesAsync(request.CategoryId, cancellationToken);

        if (category is null)
        {
            return Result.Failure(Error.NotFound("Categories.NotFound", "Kategori bulunamadı."));
        }

        var productAttribute = await productAttributeWriteRepository.GetByIdAsync(request.ProductAttributeId, cancellationToken);

        if (productAttribute is null)
        {
            return Result.Failure(Error.NotFound("ProductAttributes.NotFound", "Özellik bulunamadı."));
        }

        var result = category.AssignAttribute(request.ProductAttributeId);

        if (result.IsFailure)
        {
            return result;
        }

        await categoryWriteRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
