using ECommercePlatform.Api.Common;
using ECommercePlatform.Modules.Promotion.Application.Coupons;
using ECommercePlatform.Modules.Promotion.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

public sealed record CreateCouponRequest(
    string Code,
    CouponDiscountType DiscountType,
    decimal DiscountValue,
    DateTime ValidFrom,
    DateTime ValidTo,
    int? UsageLimit);

[ApiController]
[Route("api/coupons")]
[Authorize(Roles = "Admin")]
public sealed class CouponsController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateCouponRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateCouponCommand(
            request.Code, request.DiscountType, request.DiscountValue, request.ValidFrom, request.ValidTo, request.UsageLimit);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetAll), null, new { id = result.Value })
            : result.ToProblemDetails();
    }

    [HttpPost("{couponId:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid couponId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeactivateCouponCommand(couponId), cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblemDetails();
    }

    [HttpPost("{couponId:guid}/reactivate")]
    public async Task<IActionResult> Reactivate(Guid couponId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ReactivateCouponCommand(couponId), cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblemDetails();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetCouponsQuery(pageNumber, pageSize), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
    }
}
