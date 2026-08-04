using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.BuildingBlocks.Application.Models;
using ECommercePlatform.Modules.Promotion.Application.Abstractions;
using ECommercePlatform.Modules.Promotion.Application.Dtos;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Promotion.Application.Coupons;

public sealed class GetCouponsQueryHandler(ICouponReadRepository couponReadRepository)
    : IQueryHandler<GetCouponsQuery, PagedResult<CouponDto>>
{
    public async Task<Result<PagedResult<CouponDto>>> Handle(GetCouponsQuery request, CancellationToken cancellationToken)
    {
        var result = await couponReadRepository.GetPagedAsync(request.PageNumber, request.PageSize, cancellationToken);

        return Result.Success(result);
    }
}
