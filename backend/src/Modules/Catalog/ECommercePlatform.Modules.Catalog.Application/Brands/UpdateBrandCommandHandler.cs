using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Catalog.Application.Abstractions;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Catalog.Application.Brands;

public sealed class UpdateBrandCommandHandler(IBrandWriteRepository brandWriteRepository)
    : ICommandHandler<UpdateBrandCommand>
{
    public async Task<Result> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await brandWriteRepository.GetByIdAsync(request.BrandId, cancellationToken);

        if (brand is null)
        {
            return Result.Failure(Error.NotFound("Brands.NotFound", "Marka bulunamadı."));
        }

        brand.Rename(request.Name);

        await brandWriteRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
