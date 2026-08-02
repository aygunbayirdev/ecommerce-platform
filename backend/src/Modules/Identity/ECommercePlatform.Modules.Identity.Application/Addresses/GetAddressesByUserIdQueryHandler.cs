using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Identity.Application.Abstractions;
using ECommercePlatform.Modules.Identity.Application.Dtos;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Identity.Application.Addresses;

public sealed class GetAddressesByUserIdQueryHandler(IAddressReadRepository addressReadRepository)
    : IQueryHandler<GetAddressesByUserIdQuery, IReadOnlyList<AddressDto>>
{
    public async Task<Result<IReadOnlyList<AddressDto>>> Handle(GetAddressesByUserIdQuery request, CancellationToken cancellationToken)
    {
        var addresses = await addressReadRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        return Result.Success(addresses);
    }
}
