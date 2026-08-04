using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Review.Application.Abstractions;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Review.Application.Reviews;

public sealed class ApproveReviewCommandHandler(IReviewWriteRepository reviewWriteRepository)
    : ICommandHandler<ApproveReviewCommand>
{
    public async Task<Result> Handle(ApproveReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await reviewWriteRepository.GetByIdAsync(request.ReviewId, cancellationToken);

        if (review is null)
        {
            return Result.Failure(Error.NotFound("Reviews.NotFound", "Yorum bulunamadı."));
        }

        var result = review.Approve();

        if (result.IsFailure)
        {
            return result;
        }

        await reviewWriteRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
