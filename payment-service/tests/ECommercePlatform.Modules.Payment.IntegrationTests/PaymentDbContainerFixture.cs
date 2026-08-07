using ECommercePlatform.Modules.Payment.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace ECommercePlatform.Modules.Payment.IntegrationTests;

public sealed class PaymentDbContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17").Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public PaymentDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<PaymentDbContext>();
        optionsBuilder
            .UseNpgsql(ConnectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", PaymentDbContext.Schema))
            .UseSnakeCaseNamingConvention();

        return new PaymentDbContext(optionsBuilder.Options);
    }
}

[CollectionDefinition(nameof(PaymentDbCollection))]
public sealed class PaymentDbCollection : ICollectionFixture<PaymentDbContainerFixture>
{
}
