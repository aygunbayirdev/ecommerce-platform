using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.BuildingBlocks.Application.Models;
using ECommercePlatform.Modules.Review.Application.Abstractions;
using ECommercePlatform.Modules.Review.Application.Dtos;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Review.Application.Reviews;

public sealed class GetPendingReviewsQueryHandler(IReviewReadRepository reviewReadRepository)
    : IQueryHandler<GetPendingReviewsQuery, PagedResult<ReviewDto>>
{
    public async Task<Result<PagedResult<ReviewDto>>> Handle(GetPendingReviewsQuery request, CancellationToken cancellationToken)
    {
        var result = await reviewReadRepository.GetPendingAsync(request.PageNumber, request.PageSize, cancellationToken);

        return Result.Success(result);
    }
}
