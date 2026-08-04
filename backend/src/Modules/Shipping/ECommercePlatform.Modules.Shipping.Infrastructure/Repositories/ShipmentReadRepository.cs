using System.Data;
using Dapper;
using ECommercePlatform.BuildingBlocks.Infrastructure.Persistence;
using ECommercePlatform.Modules.Shipping.Application.Abstractions;
using ECommercePlatform.Modules.Shipping.Application.Dtos;
using ECommercePlatform.Modules.Shipping.Domain;

namespace ECommercePlatform.Modules.Shipping.Infrastructure.Repositories;

internal sealed class ShipmentReadRepository(ISqlConnectionFactory connectionFactory) : IShipmentReadRepository
{
    private sealed record ShipmentRow(
        Guid Id,
        Guid OrderId,
        string Carrier,
        string TrackingNumber,
        ShipmentStatus Status,
        string? FailureReason,
        DateTime CreatedAtUtc,
        DateTime UpdatedAtUtc);

    private sealed record StatusHistoryRow(ShipmentStatus Status, string? Note, DateTime ChangedAtUtc);

    public async Task<ShipmentDto?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                id AS "Id", order_id AS "OrderId", carrier AS "Carrier", tracking_number AS "TrackingNumber",
                status AS "Status", failure_reason AS "FailureReason",
                created_at_utc AS "CreatedAtUtc", updated_at_utc AS "UpdatedAtUtc"
            FROM shipping.shipments
            WHERE order_id = @OrderId;

            SELECT status AS "Status", note AS "Note", changed_at_utc AS "ChangedAtUtc"
            FROM shipping.shipment_status_history
            WHERE shipment_id = (SELECT id FROM shipping.shipments WHERE order_id = @OrderId)
            ORDER BY changed_at_utc;
            """;

        using IDbConnection connection = connectionFactory.CreateConnection();

        var command = new CommandDefinition(sql, new { OrderId = orderId }, cancellationToken: cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);

        var shipment = await multi.ReadSingleOrDefaultAsync<ShipmentRow>();

        if (shipment is null)
        {
            return null;
        }

        var statusHistory = (await multi.ReadAsync<StatusHistoryRow>())
            .Select(h => new ShipmentStatusHistoryDto(h.Status.ToString(), h.Note, h.ChangedAtUtc))
            .ToList();

        return new ShipmentDto(
            shipment.Id,
            shipment.OrderId,
            shipment.Carrier,
            shipment.TrackingNumber,
            shipment.Status.ToString(),
            shipment.FailureReason,
            shipment.CreatedAtUtc,
            shipment.UpdatedAtUtc,
            statusHistory);
    }
}
