using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.BuildingBlocks.Application.Models;
using ECommercePlatform.Modules.Review.Application.Abstractions;
using ECommercePlatform.Modules.Review.Application.Dtos;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Review.Application.Reviews;

public sealed class GetApprovedReviewsByProductIdQueryHandler(IReviewReadRepository reviewReadRepository)
    : IQueryHandler<GetApprovedReviewsByProductIdQuery, PagedResult<ReviewDto>>
{
    public async Task<Result<PagedResult<ReviewDto>>> Handle(
        GetApprovedReviewsByProductIdQuery request, CancellationToken cancellationToken)
    {
        var result = await reviewReadRepository.GetApprovedByProductIdAsync(
            request.ProductId, request.PageNumber, request.PageSize, cancellationToken);

        return Result.Success(result);
    }
}
