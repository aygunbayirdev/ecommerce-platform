using ECommercePlatform.BuildingBlocks.Application.Messaging;

namespace ECommercePlatform.Modules.Review.Application.Reviews;

public sealed record CreateReviewCommand(Guid UserId, Guid ProductId, Guid OrderId, int Rating, string Comment) : ICommand<Guid>;
