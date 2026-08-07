using ECommercePlatform.BuildingBlocks.Infrastructure.Persistence;
using ECommercePlatform.Modules.Payment.Infrastructure.Repositories;

namespace ECommercePlatform.Modules.Payment.IntegrationTests;

[Collection(nameof(PaymentDbCollection))]
public sealed class PaymentReadRepositoryTests(PaymentDbContainerFixture fixture)
{
    [Fact]
    public async Task GetByOrderIdAsync_ShouldReturnPaymentWithTransactions_MatchingWhatWasWrittenViaEfCore()
    {
        var payment = Domain.Payment.Create(Guid.NewGuid(), Guid.NewGuid(), 250m);
        var idempotencyKey = Guid.NewGuid().ToString();
        payment.Attempt(idempotencyKey, isSuccessful: true, failureReason: null);

        await using (var writeDbContext = fixture.CreateDbContext())
        {
            var writeRepository = new PaymentWriteRepository(writeDbContext);
            writeRepository.Add(payment);
            await writeRepository.SaveChangesAsync(CancellationToken.None);
        }

        var connectionFactory = new NpgsqlConnectionFactory(fixture.ConnectionString);
        var readRepository = new PaymentReadRepository(connectionFactory);

        var result = await readRepository.GetByOrderIdAsync(payment.OrderId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(payment.UserId, result!.UserId);
        Assert.Equal(250m, result.Amount);
        Assert.Equal("Succeeded", result.Status);
        var transaction = Assert.Single(result.Transactions);
        Assert.Equal(idempotencyKey, transaction.IdempotencyKey);
        Assert.True(transaction.IsSuccessful);
    }

    [Fact]
    public async Task GetByOrderIdAsync_ShouldReturnNull_WhenNoPaymentExistsForThatOrder()
    {
        var connectionFactory = new NpgsqlConnectionFactory(fixture.ConnectionString);
        var readRepository = new PaymentReadRepository(connectionFactory);

        var result = await readRepository.GetByOrderIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
    }
}
