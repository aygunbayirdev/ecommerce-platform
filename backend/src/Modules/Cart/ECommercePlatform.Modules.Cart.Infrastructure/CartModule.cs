using ECommercePlatform.BuildingBlocks.Infrastructure;
using ECommercePlatform.BuildingBlocks.Infrastructure.Outbox;
using ECommercePlatform.Modules.Cart.Application;
using ECommercePlatform.Modules.Cart.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommercePlatform.Modules.Cart.Infrastructure;

public static class CartModule
{
    public static IServiceCollection AddCartModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("'Default' connection string is not configured.");

        services.AddDomainEventOutbox();

        services.AddDbContext<CartDbContext>((sp, options) =>
            options
                .UseNpgsql(connectionString, npgsqlOptions =>
                    npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", CartDbContext.Schema))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetRequiredService<OutboxWritingInterceptor>()));

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CartApplicationAssemblyMarker).Assembly));
        services.AddValidatorsFromAssembly(typeof(CartApplicationAssemblyMarker).Assembly);

        services.AddOutboxProcessor<CartDbContext>();

        return services;
    }
}
