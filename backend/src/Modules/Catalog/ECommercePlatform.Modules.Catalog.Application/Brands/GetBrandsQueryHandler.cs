using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Catalog.Application.Abstractions;
using ECommercePlatform.Modules.Catalog.Application.Dtos;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Catalog.Application.Brands;

public sealed class GetBrandsQueryHandler(IBrandReadRepository brandReadRepository)
    : IQueryHandler<GetBrandsQuery, IReadOnlyList<BrandDto>>
{
    public async Task<Result<IReadOnlyList<BrandDto>>> Handle(GetBrandsQuery request, CancellationToken cancellationToken)
    {
        var brands = await brandReadRepository.GetAllAsync(cancellationToken);

        return Result.Success(brands);
    }
}
