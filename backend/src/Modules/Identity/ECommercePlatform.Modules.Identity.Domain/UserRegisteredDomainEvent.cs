using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Identity.Domain;

public sealed record UserRegisteredDomainEvent(Guid UserId, string Email) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
