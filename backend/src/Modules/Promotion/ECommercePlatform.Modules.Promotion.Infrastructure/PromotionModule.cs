using ECommercePlatform.BuildingBlocks.Infrastructure;
using ECommercePlatform.BuildingBlocks.Infrastructure.Outbox;
using ECommercePlatform.Modules.Promotion.Application;
using ECommercePlatform.Modules.Promotion.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommercePlatform.Modules.Promotion.Infrastructure;

public static class PromotionModule
{
    public static IServiceCollection AddPromotionModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("'Default' connection string is not configured.");

        services.AddDomainEventOutbox();

        services.AddDbContext<PromotionDbContext>((sp, options) =>
            options
                .UseNpgsql(connectionString, npgsqlOptions =>
                    npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", PromotionDbContext.Schema))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetRequiredService<OutboxWritingInterceptor>()));

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(PromotionApplicationAssemblyMarker).Assembly));
        services.AddValidatorsFromAssembly(typeof(PromotionApplicationAssemblyMarker).Assembly);

        services.AddOutboxProcessor<PromotionDbContext>();

        return services;
    }
}
