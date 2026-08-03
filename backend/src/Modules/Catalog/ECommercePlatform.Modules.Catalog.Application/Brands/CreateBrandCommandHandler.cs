using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Catalog.Application.Abstractions;
using ECommercePlatform.Modules.Catalog.Domain;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Catalog.Application.Brands;

public sealed class CreateBrandCommandHandler(IBrandWriteRepository brandWriteRepository)
    : ICommandHandler<CreateBrandCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = Brand.Create(request.Name);

        brandWriteRepository.Add(brand);
        await brandWriteRepository.SaveChangesAsync(cancellationToken);

        return brand.Id;
    }
}
