using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Review.Application.Reviews;

public sealed record ApproveReviewCommand(Guid ReviewId) : ICommand;
