using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Review.Application.Abstractions;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Review.Application.Reviews;

/// <summary>
/// Rejecting a pending review deletes it outright — unlike Approve, there's no "already
/// rejected" state to guard against (no tri-state moderation status in the domain model).
/// </summary>
public sealed class RejectReviewCommandHandler(IReviewWriteRepository reviewWriteRepository)
    : ICommandHandler<RejectReviewCommand>
{
    public async Task<Result> Handle(RejectReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await reviewWriteRepository.GetByIdAsync(request.ReviewId, cancellationToken);

        if (review is null)
        {
            return Result.Failure(Error.NotFound("Reviews.NotFound", "Yorum bulunamadı."));
        }

        reviewWriteRepository.Remove(review);

        await reviewWriteRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
