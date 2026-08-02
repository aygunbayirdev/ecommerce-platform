namespace ECommercePlatform.SharedKernel;

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
