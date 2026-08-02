using System.Text.Json;
using ECommercePlatform.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ECommercePlatform.BuildingBlocks.Infrastructure.Outbox;

public sealed class OutboxWritingInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        AddOutboxMessages(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        AddOutboxMessages(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void AddOutboxMessages(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var entitiesWithEvents = context.ChangeTracker
            .Entries<BaseEntity>()
            .Select(entry => entry.Entity)
            .Where(entity => entity.DomainEvents.Count > 0)
            .ToList();

        if (entitiesWithEvents.Count == 0)
        {
            return;
        }

        var outboxMessages = entitiesWithEvents
            .SelectMany(entity =>
            {
                var domainEvents = entity.DomainEvents.ToList();
                entity.ClearDomainEvents();
                return domainEvents;
            })
            .Select(domainEvent => new OutboxMessage
            {
                Id = Guid.CreateVersion7(),
                Type = domainEvent.GetType().AssemblyQualifiedName!,
                Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                OccurredOnUtc = domainEvent.OccurredOnUtc,
            })
            .ToList();

        context.Set<OutboxMessage>().AddRange(outboxMessages);
    }
}
