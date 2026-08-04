using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ECommercePlatform.Modules.Payment.Application.Payments;
using ECommercePlatform.PaymentService.Api.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.PaymentService.Api.Controllers;

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
        var result = await sender.Send(new GetPaymentByOrderIdQuery(orderId), cancellationToken);

        if (result.IsFailure)
        {
            return result.ToProblemDetails();
        }

        // Faz 4: ownership used to require asking Order synchronously (GetOrderByIdQuery) — Payment
        // now carries its own copy of UserId (from OrderReadyForPaymentIntegrationEvent), so this
        // check is entirely local. No call back into the monolith, ever.
        if (result.Value.UserId != CurrentUserId && !User.IsInRole("Admin"))
        {
            return NotFound();
        }

        return Ok(result.Value);
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
}
