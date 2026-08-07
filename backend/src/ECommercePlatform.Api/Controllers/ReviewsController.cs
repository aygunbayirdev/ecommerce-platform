using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ECommercePlatform.Api.Common;
using ECommercePlatform.BuildingBlocks.Application.Models;
using ECommercePlatform.Modules.Catalog.Application.Products;
using ECommercePlatform.Modules.Identity.Application.Users;
using ECommercePlatform.Modules.Review.Application.Reviews;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

public sealed record CreateReviewRequest(Guid ProductId, Guid OrderId, int Rating, string Comment);

public sealed record ReviewAdminDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    Guid UserId,
    string ReviewerName,
    Guid OrderId,
    int Rating,
    string Comment,
    DateTime CreatedAtUtc);

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

    [HttpDelete("{reviewId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reject(Guid reviewId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RejectReviewCommand(reviewId), cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblemDetails();
    }

    [HttpGet("by-product/{productId:guid}")]
    public async Task<IActionResult> GetByProduct(
        Guid productId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetApprovedReviewsByProductIdQuery(productId, pageNumber, pageSize), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
    }

    // Enriches the raw ReviewDto list (ProductId/UserId only, no display names) with product name
    // and reviewer name — composition happens here, not in Review.Application, so the module never
    // needs a reference to Catalog.Application or Identity.Application (same reasoning as
    // StockItemsController.GetAll after the Faz 5 Inventory<->Catalog circular-dependency fix).
    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPending(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetPendingReviewsQuery(pageNumber, pageSize), cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ToProblemDetails();
        }

        var page = result.Value;

        if (page.Items.Count == 0)
        {
            return Ok(new PagedResult<ReviewAdminDto>
            {
                Items = [],
                PageNumber = page.PageNumber,
                PageSize = page.PageSize,
                TotalCount = page.TotalCount,
            });
        }

        var productNameById = new Dictionary<Guid, string>();
        foreach (var productId in page.Items.Select(r => r.ProductId).Distinct())
        {
            var productResult = await sender.Send(new GetProductByIdQuery(productId), cancellationToken);
            productNameById[productId] = productResult.IsSuccess ? productResult.Value.Name : "(bilinmiyor)";
        }

        var reviewerNameByUserId = new Dictionary<Guid, string>();
        foreach (var userId in page.Items.Select(r => r.UserId).Distinct())
        {
            var userResult = await sender.Send(new GetUserByIdQuery(userId), cancellationToken);
            reviewerNameByUserId[userId] = userResult.IsSuccess
                ? $"{userResult.Value.FirstName} {userResult.Value.LastName}"
                : "(bilinmiyor)";
        }

        var items = page.Items
            .Select(r => new ReviewAdminDto(
                r.Id,
                r.ProductId,
                productNameById[r.ProductId],
                r.UserId,
                reviewerNameByUserId[r.UserId],
                r.OrderId,
                r.Rating,
                r.Comment,
                r.CreatedAtUtc))
            .ToList();

        return Ok(new PagedResult<ReviewAdminDto>
        {
            Items = items,
            PageNumber = page.PageNumber,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount,
        });
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
}
