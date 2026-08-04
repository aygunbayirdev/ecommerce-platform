using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ECommercePlatform.BuildingBlocks.Messaging;

/// <summary>
/// A generic RabbitMQ consumer: subscribes to <paramref name="queueName"/>, JSON-deserializes each
/// message into <typeparamref name="TMessage"/>, and invokes <paramref name="handleAsync"/> inside a
/// fresh DI scope (so the handler can resolve scoped services like ISender/DbContext, mirroring how
/// every command handler in this codebase gets its dependencies). Ack's on success; nack's WITHOUT
/// requeue on failure after logging — a poison message is dead-lettered rather than retried forever,
/// since this project doesn't set up a dead-letter queue (a known, documented limitation, not an
/// oversight — see the Faz 4 writeup).
///
/// Generalized here (rather than one bespoke class per consumer) because there are two real call
/// sites: Payment.Service consuming OrderReadyForPaymentIntegrationEvent, and Order consuming
/// PaymentSucceededIntegrationEvent — the same "two or more real usages justify an abstraction" bar
/// OutboxProcessor&lt;TDbContext&gt; already cleared for the in-process outbox.
/// </summary>
public sealed class RabbitMqConsumerBackgroundService<TMessage>(
    IServiceProvider rootServiceProvider,
    IConnection connection,
    string queueName,
    Func<TMessage, IServiceProvider, CancellationToken, Task> handleAsync,
    ILogger<RabbitMqConsumerBackgroundService<TMessage>> logger) : BackgroundService
{
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
        var consumer = new AsyncEventingBasicConsumer(_channel!);

        consumer.ReceivedAsync += async (_, delivery) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(delivery.Body.Span);
                var message = JsonSerializer.Deserialize<TMessage>(json)
                    ?? throw new InvalidOperationException($"Deserialized {typeof(TMessage).Name} was null.");

                using var scope = rootServiceProvider.CreateScope();
                await handleAsync(message, scope.ServiceProvider, stoppingToken);

                await _channel!.BasicAckAsync(delivery.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to handle {MessageType} from {QueueName}", typeof(TMessage).Name, queueName);
                await _channel!.BasicNackAsync(delivery.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
            }
        };

        await _channel!.BasicConsumeAsync(queueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

        // BasicConsumeAsync registers the consumer and returns immediately — this await keeps the
        // BackgroundService alive for the host's lifetime instead of ExecuteAsync completing right away.
        await Task.Delay(Timeout.Infinite, stoppingToken).ContinueWith(_ => { }, TaskScheduler.Default);
    }
}
