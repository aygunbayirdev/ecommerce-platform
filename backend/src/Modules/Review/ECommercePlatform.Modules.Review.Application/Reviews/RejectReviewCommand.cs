using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Review.Application.Reviews;

public sealed record RejectReviewCommand(Guid ReviewId) : ICommand;
