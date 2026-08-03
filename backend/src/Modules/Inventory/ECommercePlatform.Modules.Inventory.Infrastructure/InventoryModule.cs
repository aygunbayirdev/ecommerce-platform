using ECommercePlatform.BuildingBlocks.Infrastructure;
using ECommercePlatform.BuildingBlocks.Infrastructure.Outbox;
using ECommercePlatform.Modules.Inventory.Application;
using ECommercePlatform.Modules.Inventory.Application.Abstractions;
using ECommercePlatform.Modules.Inventory.Infrastructure.Persistence;
using ECommercePlatform.Modules.Inventory.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommercePlatform.Modules.Inventory.Infrastructure;

public static class InventoryModule
{
    public static IServiceCollection AddInventoryModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("'Default' connection string is not configured.");

        services.AddDomainEventOutbox();

        services.AddDbContext<InventoryDbContext>((sp, options) =>
            options
                .UseNpgsql(connectionString, npgsqlOptions =>
                    npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", InventoryDbContext.Schema))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetRequiredService<OutboxWritingInterceptor>()));

        services.AddScoped<IStockItemWriteRepository, StockItemWriteRepository>();
        services.AddScoped<IStockItemReadRepository, StockItemReadRepository>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(InventoryApplicationAssemblyMarker).Assembly));
        services.AddValidatorsFromAssembly(typeof(InventoryApplicationAssemblyMarker).Assembly);

        // No AddOutboxProcessor<InventoryDbContext>() yet — this module has no entities/domain events
        // to process. Register it once the module raises its first domain event (see TASKS.md).

        return services;
    }
}
