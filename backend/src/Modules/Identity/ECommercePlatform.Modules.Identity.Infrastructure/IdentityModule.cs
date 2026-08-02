using ECommercePlatform.BuildingBlocks.Infrastructure;
using ECommercePlatform.BuildingBlocks.Infrastructure.Outbox;
using ECommercePlatform.Modules.Identity.Application;
using ECommercePlatform.Modules.Identity.Application.Abstractions;
using ECommercePlatform.Modules.Identity.Infrastructure.Persistence;
using ECommercePlatform.Modules.Identity.Infrastructure.Repositories;
using ECommercePlatform.Modules.Identity.Infrastructure.Security;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommercePlatform.Modules.Identity.Infrastructure;

public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("'Default' connection string is not configured.");

        services.AddDomainEventOutbox();

        services.AddDbContext<IdentityDbContext>((sp, options) =>
            options
                .UseNpgsql(connectionString, npgsqlOptions =>
                    npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", IdentityDbContext.Schema))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetRequiredService<OutboxWritingInterceptor>()));

        services.AddScoped<IUserWriteRepository, UserWriteRepository>();
        services.AddScoped<IUserReadRepository, UserReadRepository>();
        services.AddScoped<IRefreshTokenWriteRepository, RefreshTokenWriteRepository>();
        services.AddSingleton<IPasswordHasher, PasswordHasherAdapter>();
        services.AddScoped<ITokenGenerator, JwtTokenGenerator>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IdentityApplicationAssemblyMarker).Assembly));
        services.AddValidatorsFromAssembly(typeof(IdentityApplicationAssemblyMarker).Assembly);

        services.AddOutboxProcessor<IdentityDbContext>();

        return services;
    }
}
