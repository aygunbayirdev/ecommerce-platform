using ECommercePlatform.Modules.Payment.Domain;
using ECommercePlatform.Modules.Payment.Infrastructure.Repositories;

namespace ECommercePlatform.Modules.Payment.IntegrationTests;

[Collection(nameof(PaymentDbCollection))]
public sealed class PaymentWriteRepositoryTests(PaymentDbContainerFixture fixture)
{
    [Fact]
    public async Task AddAndSave_ShouldPersistPaymentWithTransactions_ToRealPostgres()
    {
        var payment = Domain.Payment.Create(Guid.NewGuid(), Guid.NewGuid(), 150m);
        var declinedKey = Guid.NewGuid().ToString();
        var succeededKey = Guid.NewGuid().ToString();
        payment.Attempt(declinedKey, isSuccessful: false, failureReason: "Kart reddedildi.");
        payment.Attempt(succeededKey, isSuccessful: true, failureReason: null);

        await using (var writeDbContext = fixture.CreateDbContext())
        {
            var repository = new PaymentWriteRepository(writeDbContext);
            repository.Add(payment);
            await repository.SaveChangesAsync(CancellationToken.None);
        }

        await using var readDbContext = fixture.CreateDbContext();
        var readRepository = new PaymentWriteRepository(readDbContext);
        var persisted = await readRepository.GetByOrderIdAsync(payment.OrderId, CancellationToken.None);

        Assert.NotNull(persisted);
        Assert.Equal(PaymentStatus.Succeeded, persisted!.Status);
        Assert.Equal(150m, persisted.Amount);
        Assert.Equal(2, persisted.Transactions.Count);
        Assert.Contains(persisted.Transactions, t => t.IdempotencyKey == declinedKey && !t.IsSuccessful);
        Assert.Contains(persisted.Transactions, t => t.IdempotencyKey == succeededKey && t.IsSuccessful);
    }

    [Fact]
    public async Task GetByOrderIdAsync_ShouldReturnNull_WhenNoPaymentExistsForThatOrder()
    {
        await using var dbContext = fixture.CreateDbContext();
        var repository = new PaymentWriteRepository(dbContext);

        var result = await repository.GetByOrderIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
    }
}
