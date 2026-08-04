using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.BuildingBlocks.Application.Models;
using ECommercePlatform.Modules.Promotion.Application.Dtos;

namespace ECommercePlatform.Modules.Promotion.Application.Coupons;

public sealed record GetCouponsQuery(int PageNumber, int PageSize) : IQuery<PagedResult<CouponDto>>;
