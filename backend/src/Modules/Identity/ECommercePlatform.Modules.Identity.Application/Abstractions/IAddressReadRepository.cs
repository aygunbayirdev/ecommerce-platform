using ECommercePlatform.Modules.Identity.Application.Dtos;

namespace ECommercePlatform.Modules.Identity.Application.Abstractions;

public interface IAddressReadRepository
{
    Task<IReadOnlyList<AddressDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
