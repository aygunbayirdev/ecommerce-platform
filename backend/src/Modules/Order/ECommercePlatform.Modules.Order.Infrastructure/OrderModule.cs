using ECommercePlatform.BuildingBlocks.Infrastructure;
using ECommercePlatform.BuildingBlocks.Infrastructure.Outbox;
using ECommercePlatform.BuildingBlocks.Messaging;
using ECommercePlatform.IntegrationEvents;
using ECommercePlatform.Modules.Order.Application;
using ECommercePlatform.Modules.Order.Application.Abstractions;
using ECommercePlatform.Modules.Order.Application.Orders;
using ECommercePlatform.Modules.Order.Infrastructure.Persistence;
using ECommercePlatform.Modules.Order.Infrastructure.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommercePlatform.Modules.Order.Infrastructure;

public static class OrderModule
{
    public static IServiceCollection AddOrderModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("'Default' connection string is not configured.");

        services.AddDomainEventOutbox();

        services.AddDbContext<OrderDbContext>((sp, options) =>
            options
                .UseNpgsql(connectionString, npgsqlOptions =>
                    npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", OrderDbContext.Schema))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetRequiredService<OutboxWritingInterceptor>()));

        services.AddScoped<IOrderWriteRepository, OrderWriteRepository>();
        services.AddScoped<IOrderReadRepository, OrderReadRepository>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(OrderApplicationAssemblyMarker).Assembly));
        services.AddValidatorsFromAssembly(typeof(OrderApplicationAssemblyMarker).Assembly);

        // Unlike every other module's outbox (dispatched in-process via MediatR), 100% of Order's
        // domain events are integration events meant to leave the process — Order.MarkReadyForPayment()
        // is the only thing that ever raises one. So this outbox is wired straight to RabbitMQ instead
        // of AddOutboxProcessor<OrderDbContext>() (see Faz 4 / CLAUDE.md madde 4).
        services.AddRabbitMqOutboxPublisher<OrderDbContext>(queueName: "order-ready-for-payment");

        // Symmetric consumer side: Payment.Service publishes this once a charge succeeds. Order
        // never calls Payment — it only reacts to this event, same as it never called Payment before.
        services.AddRabbitMqConsumer<PaymentSucceededIntegrationEvent>(
            queueName: "payment-succeeded",
            handleAsync: async (message, sp, cancellationToken) =>
            {
                var sender = sp.GetRequiredService<ISender>();
                await sender.Send(new MarkOrderAsPaidCommand(message.OrderId), cancellationToken);
            });

        return services;
    }
}
