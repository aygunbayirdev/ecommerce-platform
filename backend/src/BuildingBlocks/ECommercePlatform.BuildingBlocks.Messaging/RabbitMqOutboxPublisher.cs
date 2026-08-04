using System.Text;
using ECommercePlatform.BuildingBlocks.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace ECommercePlatform.BuildingBlocks.Messaging;

/// <summary>
/// The RabbitMQ counterpart to the in-process OutboxProcessor&lt;TDbContext&gt;
/// (BuildingBlocks.Infrastructure/Outbox/OutboxProcessor.cs): same polling loop over the same
/// outbox_messages table — the transactional write-side guarantee (OutboxWritingInterceptor, shared
/// and unchanged) still applies — but this publishes the raw JSON payload to a RabbitMQ queue instead
/// of dispatching a DomainEventNotification&lt;T&gt; via MediatR.
///
/// Kept as a separate class rather than making OutboxProcessor's dispatch target pluggable, so this
/// change carries zero risk to Identity/Catalog's already-proven in-process outbox flow. A module
/// wires ONE of the two (never both) for a given DbContext — see AddRabbitMqOutboxPublisher.
/// </summary>
public sealed class RabbitMqOutboxPublisher<TDbContext>(
    IServiceScopeFactory serviceScopeFactory,
    IConnection connection,
    string queueName,
    ILogger<RabbitMqOutboxPublisher<TDbContext>> logger) : BackgroundService
    where TDbContext : DbContext
{
    private const int BatchSize = 20;
    private const int MaxRetryCount = 10;
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);

    private IChannel? _channel;

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await _channel.QueueDeclareAsync(
            queueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollingInterval);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await PublishOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception, "RabbitMQ outbox publishing failed for {DbContext} -> {QueueName}", typeof(TDbContext).Name, queueName);
            }
        }
    }

    private async Task PublishOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

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
                var body = Encoding.UTF8.GetBytes(message.Payload);
                var properties = new BasicProperties { Persistent = true, ContentType = "application/json" };

                await _channel!.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: queueName,
                    mandatory: false,
                    basicProperties: properties,
                    body: body,
                    cancellationToken: cancellationToken);

                message.ProcessedOnUtc = DateTime.UtcNow;
            }
            catch (Exception exception)
            {
                message.RetryCount++;
                message.Error = exception.Message;
                logger.LogError(exception, "Failed to publish outbox message {OutboxMessageId} to {QueueName}", message.Id, queueName);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
