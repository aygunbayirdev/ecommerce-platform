using ECommercePlatform.SharedKernel;
using MediatR;

namespace ECommercePlatform.BuildingBlocks.Application.Messaging;

public interface IDomainEventHandler<TDomainEvent> : INotificationHandler<DomainEventNotification<TDomainEvent>>
    where TDomainEvent : IDomainEvent;
