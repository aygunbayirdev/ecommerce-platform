using FluentValidation;

namespace ECommercePlatform.Modules.Review.Application.Reviews;

public sealed class RejectReviewCommandValidator : AbstractValidator<RejectReviewCommand>
{
    public RejectReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId).NotEmpty();
    }
}
