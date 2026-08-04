using FluentValidation;

namespace ECommercePlatform.Modules.Review.Application.Reviews;

public sealed class ApproveReviewCommandValidator : AbstractValidator<ApproveReviewCommand>
{
    public ApproveReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId).NotEmpty();
    }
}
