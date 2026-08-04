using ECommercePlatform.BuildingBlocks.Application.Models;
using ECommercePlatform.Modules.Promotion.Application.Dtos;

namespace ECommercePlatform.Modules.Promotion.Application.Abstractions;

public interface ICouponReadRepository
{
    Task<PagedResult<CouponDto>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
}
