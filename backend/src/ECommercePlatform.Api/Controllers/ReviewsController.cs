using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ECommercePlatform.Api.Common;
using ECommercePlatform.Modules.Review.Application.Reviews;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

public sealed record CreateReviewRequest(Guid ProductId, Guid OrderId, int Rating, string Comment);

[ApiController]
[Route("api/reviews")]
public sealed class ReviewsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreateReviewRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateReviewCommand(CurrentUserId, request.ProductId, request.OrderId, request.Rating, request.Comment);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetByProduct), new { productId = request.ProductId }, new { id = result.Value })
            : result.ToProblemDetails();
    }

    [HttpPost("{reviewId:guid}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(Guid reviewId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ApproveReviewCommand(reviewId), cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblemDetails();
    }

    [HttpGet("by-product/{productId:guid}")]
    public async Task<IActionResult> GetByProduct(
        Guid productId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetApprovedReviewsByProductIdQuery(productId, pageNumber, pageSize), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPending(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetPendingReviewsQuery(pageNumber, pageSize), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
}
