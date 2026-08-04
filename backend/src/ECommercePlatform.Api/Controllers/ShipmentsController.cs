using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ECommercePlatform.Api.Common;
using ECommercePlatform.Modules.Order.Application.Orders;
using ECommercePlatform.Modules.Shipping.Application.Shipments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

public sealed record CreateShipmentRequest(Guid OrderId, string Carrier, string TrackingNumber);

public sealed record MarkShipmentFailedRequest(string Reason);

[ApiController]
[Route("api/shipments")]
public sealed class ShipmentsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateShipmentRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateShipmentCommand(request.OrderId, request.Carrier, request.TrackingNumber);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetByOrderId), new { orderId = request.OrderId }, new { id = result.Value })
            : result.ToProblemDetails();
    }

    [HttpPost("{shipmentId:guid}/mark-in-transit")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> MarkInTransit(Guid shipmentId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new MarkShipmentInTransitCommand(shipmentId), cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblemDetails();
    }

    [HttpPost("{shipmentId:guid}/mark-delivered")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> MarkDelivered(Guid shipmentId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new MarkShipmentDeliveredCommand(shipmentId), cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblemDetails();
    }

    [HttpPost("{shipmentId:guid}/mark-failed")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> MarkFailed(Guid shipmentId, MarkShipmentFailedRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new MarkShipmentFailedCommand(shipmentId, request.Reason), cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblemDetails();
    }

    [HttpGet("by-order/{orderId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetByOrderId(Guid orderId, CancellationToken cancellationToken)
    {
        // Shipping doesn't know the order's owner (no UserId of its own) — ownership is checked by
        // asking Order first, the same pattern PaymentsController.GetByOrderId uses.
        var orderResult = await sender.Send(new GetOrderByIdQuery(orderId), cancellationToken);

        if (orderResult.IsFailure)
        {
            return orderResult.ToProblemDetails();
        }

        if (orderResult.Value.UserId != CurrentUserId && !User.IsInRole("Admin"))
        {
            return NotFound();
        }

        var result = await sender.Send(new GetShipmentByOrderIdQuery(orderId), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
}
