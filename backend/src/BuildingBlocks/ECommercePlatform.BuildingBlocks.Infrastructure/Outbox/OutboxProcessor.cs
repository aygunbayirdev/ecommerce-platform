using System.Text.Json;
using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.SharedKernel;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.BuildingBlocks.Infrastructure.Outbox;

public sealed class OutboxProcessor<TDbContext>(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<OutboxProcessor<TDbContext>> logger) : BackgroundService
    where TDbContext : DbContext
{
    private const int BatchSize = 20;
    private const int MaxRetryCount = 10;
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollingInterval);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Outbox processing failed for {DbContext}", typeof(TDbContext).Name);
            }
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

        var messages = await dbContext.Set<OutboxMessage>()
            .Where(message => message.ProcessedOnUtc == null && message.RetryCount < MaxRetryCount)
            .OrderBy(message => message.OccurredOnUtc)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            return;
        }

        foreach (var message in messages)
        {
            try
            {
                var domainEventType = Type.GetType(message.Type)
                    ?? throw new InvalidOperationException($"Unknown domain event type: {message.Type}");

                var domainEvent = (IDomainEvent)JsonSerializer.Deserialize(message.Payload, domainEventType)!;

                var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEventType);
                var notification = (INotification)Activator.CreateInstance(notificationType, domainEvent, message.Id)!;

                await publisher.Publish(notification, cancellationToken);

                message.ProcessedOnUtc = DateTime.UtcNow;
            }
            catch (Exception exception)
            {
                message.RetryCount++;
                message.Error = exception.Message;
                logger.LogError(exception, "Failed to process outbox message {OutboxMessageId}", message.Id);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
