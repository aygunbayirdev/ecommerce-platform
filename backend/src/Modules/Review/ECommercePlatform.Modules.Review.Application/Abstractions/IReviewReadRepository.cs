using ECommercePlatform.BuildingBlocks.Application.Models;
using ECommercePlatform.Modules.Review.Application.Dtos;

namespace ECommercePlatform.Modules.Review.Application.Abstractions;

public interface IReviewReadRepository
{
    Task<PagedResult<ReviewDto>> GetApprovedByProductIdAsync(
        Guid productId, int pageNumber, int pageSize, CancellationToken cancellationToken);

    Task<PagedResult<ReviewDto>> GetPendingAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
}
