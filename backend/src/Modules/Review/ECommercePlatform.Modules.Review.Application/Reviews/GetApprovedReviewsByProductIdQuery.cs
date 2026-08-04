using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.BuildingBlocks.Application.Models;
using ECommercePlatform.Modules.Review.Application.Dtos;

namespace ECommercePlatform.Modules.Review.Application.Reviews;

public sealed record GetApprovedReviewsByProductIdQuery(Guid ProductId, int PageNumber, int PageSize)
    : IQuery<PagedResult<ReviewDto>>;
