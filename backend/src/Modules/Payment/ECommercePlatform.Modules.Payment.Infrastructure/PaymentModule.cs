using ECommercePlatform.BuildingBlocks.Infrastructure;
using ECommercePlatform.BuildingBlocks.Infrastructure.Outbox;
using ECommercePlatform.Modules.Payment.Application;
using ECommercePlatform.Modules.Payment.Application.Abstractions;
using ECommercePlatform.Modules.Payment.Infrastructure.Gateways;
using ECommercePlatform.Modules.Payment.Infrastructure.Persistence;
using ECommercePlatform.Modules.Payment.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommercePlatform.Modules.Payment.Infrastructure;

public static class PaymentModule
{
    public static IServiceCollection AddPaymentModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("'Default' connection string is not configured.");

        services.AddDomainEventOutbox();

        services.AddDbContext<PaymentDbContext>((sp, options) =>
            options
                .UseNpgsql(connectionString, npgsqlOptions =>
                    npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", PaymentDbContext.Schema))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetRequiredService<OutboxWritingInterceptor>()));

        services.AddScoped<IPaymentWriteRepository, PaymentWriteRepository>();
        services.AddScoped<IPaymentReadRepository, PaymentReadRepository>();

        // MockPaymentGateway today; swapping in a real provider (iyzico) later means changing only
        // this one registration — see IPaymentGateway.cs for the full rationale.
        services.AddScoped<IPaymentGateway, MockPaymentGateway>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(PaymentApplicationAssemblyMarker).Assembly));
        services.AddValidatorsFromAssembly(typeof(PaymentApplicationAssemblyMarker).Assembly);

        // No AddOutboxProcessor<PaymentDbContext>() yet — this module has no entities/domain events
        // to process. Register it once the module raises its first domain event (see TASKS.md).

        return services;
    }
}
