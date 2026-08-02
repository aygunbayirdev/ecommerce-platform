using ECommercePlatform.BuildingBlocks.Infrastructure;
using ECommercePlatform.BuildingBlocks.Infrastructure.Outbox;
using ECommercePlatform.Modules.Review.Application;
using ECommercePlatform.Modules.Review.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommercePlatform.Modules.Review.Infrastructure;

public static class ReviewModule
{
    public static IServiceCollection AddReviewModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("'Default' connection string is not configured.");

        services.AddDomainEventOutbox();

        services.AddDbContext<ReviewDbContext>((sp, options) =>
            options
                .UseNpgsql(connectionString, npgsqlOptions =>
                    npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", ReviewDbContext.Schema))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetRequiredService<OutboxWritingInterceptor>()));

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ReviewApplicationAssemblyMarker).Assembly));
        services.AddValidatorsFromAssembly(typeof(ReviewApplicationAssemblyMarker).Assembly);

        services.AddOutboxProcessor<ReviewDbContext>();

        return services;
    }
}
