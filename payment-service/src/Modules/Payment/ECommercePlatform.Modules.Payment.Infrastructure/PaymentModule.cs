using ECommercePlatform.BuildingBlocks.Infrastructure;
using ECommercePlatform.BuildingBlocks.Infrastructure.Outbox;
using ECommercePlatform.BuildingBlocks.Messaging;
using ECommercePlatform.IntegrationEvents;
using ECommercePlatform.Modules.Payment.Application;
using ECommercePlatform.Modules.Payment.Application.Abstractions;
using ECommercePlatform.Modules.Payment.Application.Payments;
using ECommercePlatform.Modules.Payment.Infrastructure.Gateways;
using ECommercePlatform.Modules.Payment.Infrastructure.Persistence;
using ECommercePlatform.Modules.Payment.Infrastructure.Repositories;
using FluentValidation;
using MediatR;
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

        // Publishing side: Payment.Attempt() raises PaymentSucceededIntegrationEvent on a successful
        // charge — same reasoning as Order's outbox (see OrderModule.cs): 100% of this module's
        // domain events are meant to leave the process, so RabbitMQ, never MediatR.
        services.AddRabbitMqOutboxPublisher<PaymentDbContext>(queueName: "payment-succeeded");

        // Consuming side: the monolith's Order module publishes OrderReadyForPaymentIntegrationEvent
        // when checkout reserves stock — this is what actually creates the local Payment record
        // (CreatePendingPaymentCommand is idempotent, see its handler for why that matters here).
        services.AddRabbitMqConsumer<OrderReadyForPaymentIntegrationEvent>(
            queueName: "order-ready-for-payment",
            handleAsync: async (message, sp, cancellationToken) =>
            {
                var sender = sp.GetRequiredService<ISender>();
                await sender.Send(new CreatePendingPaymentCommand(message.OrderId, message.UserId, message.Amount), cancellationToken);
            });

        return services;
    }
}
