using System.Data;
using Dapper;
using ECommercePlatform.BuildingBlocks.Infrastructure.Persistence;
using ECommercePlatform.Modules.Payment.Application.Abstractions;
using ECommercePlatform.Modules.Payment.Application.Dtos;
using ECommercePlatform.Modules.Payment.Domain;

namespace ECommercePlatform.Modules.Payment.Infrastructure.Repositories;

internal sealed class PaymentReadRepository(ISqlConnectionFactory connectionFactory) : IPaymentReadRepository
{
    private sealed record PaymentRow(Guid Id, Guid OrderId, Guid UserId, decimal Amount, PaymentStatus Status);

    public async Task<PaymentDto?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id AS "Id", order_id AS "OrderId", user_id AS "UserId", amount AS "Amount", status AS "Status"
            FROM payment.payments
            WHERE order_id = @OrderId;

            SELECT idempotency_key AS "IdempotencyKey", is_successful AS "IsSuccessful",
                   failure_reason AS "FailureReason", occurred_at_utc AS "OccurredAtUtc"
            FROM payment.payment_transactions t
            JOIN payment.payments p ON p.id = t.payment_id
            WHERE p.order_id = @OrderId
            ORDER BY t.occurred_at_utc;
            """;

        using IDbConnection connection = connectionFactory.CreateConnection();

        var command = new CommandDefinition(sql, new { OrderId = orderId }, cancellationToken: cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);

        var payment = await multi.ReadSingleOrDefaultAsync<PaymentRow>();

        if (payment is null)
        {
            return null;
        }

        var transactions = (await multi.ReadAsync<PaymentTransactionDto>()).ToList();

        return new PaymentDto(payment.Id, payment.OrderId, payment.UserId, payment.Amount, payment.Status.ToString(), transactions);
    }
}
