using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ECommercePlatform.Api.Common;
using ECommercePlatform.Modules.Order.Application.Orders;
using ECommercePlatform.Modules.Payment.Application.Payments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

public sealed record ProcessPaymentRequest(Guid OrderId, string CardNumber, string IdempotencyKey);

[ApiController]
[Route("api/payments")]
[Authorize]
public sealed class PaymentsController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Process(ProcessPaymentRequest request, CancellationToken cancellationToken)
    {
        var command = new ProcessPaymentCommand(CurrentUserId, request.OrderId, request.CardNumber, request.IdempotencyKey);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblemDetails();
    }

    [HttpGet("by-order/{orderId:guid}")]
    public async Task<IActionResult> GetByOrderId(Guid orderId, CancellationToken cancellationToken)
    {
        // Payment doesn't know the order's owner (no UserId of its own) — ownership is checked by
        // asking Order first, the same pattern OrdersController.GetById uses.
        var orderResult = await sender.Send(new GetOrderByIdQuery(orderId), cancellationToken);

        if (orderResult.IsFailure)
        {
            return orderResult.ToProblemDetails();
        }

        if (orderResult.Value.UserId != CurrentUserId && !User.IsInRole("Admin"))
        {
            return NotFound();
        }

        var result = await sender.Send(new GetPaymentByOrderIdQuery(orderId), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
}
