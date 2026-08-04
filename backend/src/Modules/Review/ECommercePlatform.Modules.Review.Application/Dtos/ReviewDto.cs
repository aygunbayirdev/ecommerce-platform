namespace ECommercePlatform.Modules.Review.Application.Dtos;

public sealed record ReviewDto(
    Guid Id,
    Guid ProductId,
    Guid UserId,
    Guid OrderId,
    int Rating,
    string Comment,
    bool IsApproved,
    DateTime CreatedAtUtc);
