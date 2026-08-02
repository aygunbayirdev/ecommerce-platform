using ECommercePlatform.BuildingBlocks.Infrastructure;
using ECommercePlatform.BuildingBlocks.Infrastructure.Outbox;
using ECommercePlatform.Modules.Shipping.Application;
using ECommercePlatform.Modules.Shipping.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommercePlatform.Modules.Shipping.Infrastructure;

public static class ShippingModule
{
    public static IServiceCollection AddShippingModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("'Default' connection string is not configured.");

        services.AddDomainEventOutbox();

        services.AddDbContext<ShippingDbContext>((sp, options) =>
            options
                .UseNpgsql(connectionString, npgsqlOptions =>
                    npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", ShippingDbContext.Schema))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetRequiredService<OutboxWritingInterceptor>()));

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ShippingApplicationAssemblyMarker).Assembly));
        services.AddValidatorsFromAssembly(typeof(ShippingApplicationAssemblyMarker).Assembly);

        services.AddOutboxProcessor<ShippingDbContext>();

        return services;
    }
}
