using ECommercePlatform.BuildingBlocks.Infrastructure.Outbox;
using ECommercePlatform.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ECommercePlatform.BuildingBlocks.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddSqlConnectionFactory(this IServiceCollection services, string connectionString)
    {
        services.TryAddSingleton<ISqlConnectionFactory>(new NpgsqlConnectionFactory(connectionString));

        return services;
    }

    /// <summary>
    /// Every module calls this from its own Add{Module}Module() registration; TryAdd keeps it idempotent
    /// across the 9 modules sharing one OutboxWritingInterceptor instance.
    /// </summary>
    public static IServiceCollection AddDomainEventOutbox(this IServiceCollection services)
    {
        services.TryAddSingleton<OutboxWritingInterceptor>();

        return services;
    }

    public static IServiceCollection AddOutboxProcessor<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        services.AddHostedService<OutboxProcessor<TDbContext>>();

        return services;
    }
}
