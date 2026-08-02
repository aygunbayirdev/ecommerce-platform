using ECommercePlatform.SharedKernel;
using MediatR;

namespace ECommercePlatform.BuildingBlocks.Application.Messaging;

public sealed class DomainEventNotification<TDomainEvent>(TDomainEvent domainEvent, Guid outboxMessageId) : INotification
    where TDomainEvent : IDomainEvent
{
    public TDomainEvent DomainEvent { get; } = domainEvent;

    public Guid OutboxMessageId { get; } = outboxMessageId;
}
