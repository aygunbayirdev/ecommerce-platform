using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ECommercePlatform.Api.Common;
using ECommercePlatform.Modules.Order.Application.Orders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

public sealed record CreateOrderRequest(Guid AddressId, string? CouponCode = null);

public sealed record CancelOrderRequest(string Reason);

[ApiController]
[Route("api/orders")]
[Authorize]
public sealed class OrdersController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateOrderCommand(CurrentUserId, request.AddressId, request.CouponCode), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { orderId = result.Value }, new { id = result.Value })
            : result.ToProblemDetails();
    }

    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> GetById(Guid orderId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOrderByIdQuery(orderId), cancellationToken);

        if (result.IsFailure)
        {
            return result.ToProblemDetails();
        }

        if (result.Value.UserId != CurrentUserId && !User.IsInRole("Admin"))
        {
            return NotFound();
        }

        return Ok(result.Value);
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetOrdersByUserIdQuery(CurrentUserId, pageNumber, pageSize), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
    }

    [HttpPost("{orderId:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid orderId, CancelOrderRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CancelMyOrderCommand(CurrentUserId, orderId, request.Reason), cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblemDetails();
    }

    // Stand-in endpoints until Payment/Shipping modules exist — see MarkOrderAsPaidCommandHandler.
    [HttpPost("{orderId:guid}/mark-paid")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> MarkAsPaid(Guid orderId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new MarkOrderAsPaidCommand(orderId), cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblemDetails();
    }

    [HttpPost("{orderId:guid}/mark-preparing")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> MarkAsPreparing(Guid orderId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new MarkOrderAsPreparingCommand(orderId), cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblemDetails();
    }

    [HttpPost("{orderId:guid}/mark-shipped")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> MarkAsShipped(Guid orderId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new MarkOrderAsShippedCommand(orderId), cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblemDetails();
    }

    [HttpPost("{orderId:guid}/mark-delivered")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> MarkAsDelivered(Guid orderId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new MarkOrderAsDeliveredCommand(orderId), cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblemDetails();
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
}
