using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace ECommercePlatform.BuildingBlocks.Messaging;

public static class MessagingServiceCollectionExtensions
{
    /// <summary>Called once per host (Program.cs) — every module in that host shares one RabbitMQ connection (TryAdd keeps this idempotent).</summary>
    public static IServiceCollection AddRabbitMqMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));

        services.TryAddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
            var factory = new ConnectionFactory
            {
                HostName = options.HostName,
                Port = options.Port,
                UserName = options.UserName,
                Password = options.Password,
            };

            // Startup-only blocking call: RabbitMQ.Client v7's connection API is async-only, but a DI
            // singleton factory delegate is synchronous. This runs once, before the host starts serving
            // requests, so blocking here doesn't cost anything on a hot path.
            return factory.CreateConnectionAsync().GetAwaiter().GetResult();
        });

        return services;
    }

    /// <summary>A module calls this for its own DbContext when ALL of that module's domain events are meant to leave the process (see Order/Payment's registrations).</summary>
    public static IServiceCollection AddRabbitMqOutboxPublisher<TDbContext>(this IServiceCollection services, string queueName)
        where TDbContext : DbContext
    {
        services.AddHostedService(sp => new RabbitMqOutboxPublisher<TDbContext>(
            sp.GetRequiredService<IServiceScopeFactory>(),
            sp.GetRequiredService<IConnection>(),
            queueName,
            sp.GetRequiredService<ILogger<RabbitMqOutboxPublisher<TDbContext>>>()));

        return services;
    }

    public static IServiceCollection AddRabbitMqConsumer<TMessage>(
        this IServiceCollection services,
        string queueName,
        Func<TMessage, IServiceProvider, CancellationToken, Task> handleAsync)
    {
        services.AddHostedService(sp => new RabbitMqConsumerBackgroundService<TMessage>(
            sp,
            sp.GetRequiredService<IConnection>(),
            queueName,
            handleAsync,
            sp.GetRequiredService<ILogger<RabbitMqConsumerBackgroundService<TMessage>>>()));

        return services;
    }
}
