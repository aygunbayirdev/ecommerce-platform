using System.Data;
using Dapper;
using ECommercePlatform.BuildingBlocks.Application.Models;
using ECommercePlatform.BuildingBlocks.Infrastructure.Persistence;
using ECommercePlatform.Modules.Order.Application.Abstractions;
using ECommercePlatform.Modules.Order.Application.Dtos;
using ECommercePlatform.Modules.Order.Domain;

namespace ECommercePlatform.Modules.Order.Infrastructure.Repositories;

internal sealed class OrderReadRepository(ISqlConnectionFactory connectionFactory) : IOrderReadRepository
{
    private sealed record OrderRow(
        Guid Id,
        string OrderNumber,
        Guid UserId,
        OrderStatus Status,
        string ShippingRecipientName,
        string ShippingPhoneNumber,
        string ShippingCity,
        string ShippingDistrict,
        string ShippingFullAddressLine,
        string ShippingPostalCode,
        string? CouponCode,
        decimal DiscountAmount,
        DateTime CreatedAtUtc);

    private sealed record StatusHistoryRow(OrderStatus Status, string? Note, DateTime ChangedAtUtc);

    private sealed record OrderSummaryRow(Guid Id, string OrderNumber, OrderStatus Status, decimal Total, DateTime CreatedAtUtc);

    public async Task<OrderDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                id AS "Id", order_number AS "OrderNumber", user_id AS "UserId", status AS "Status",
                shipping_recipient_name AS "ShippingRecipientName", shipping_phone_number AS "ShippingPhoneNumber",
                shipping_city AS "ShippingCity", shipping_district AS "ShippingDistrict",
                shipping_full_address_line AS "ShippingFullAddressLine", shipping_postal_code AS "ShippingPostalCode",
                coupon_code AS "CouponCode", discount_amount AS "DiscountAmount",
                created_at_utc AS "CreatedAtUtc"
            FROM "order".orders
            WHERE id = @Id;

            SELECT
                product_variant_id AS "ProductVariantId", product_name AS "ProductName", sku AS "Sku",
                unit_price AS "UnitPrice", quantity AS "Quantity", (unit_price * quantity) AS "LineTotal",
                NULL::uuid AS "ProductId"
            FROM "order".order_items
            WHERE order_id = @Id;

            SELECT status AS "Status", note AS "Note", changed_at_utc AS "ChangedAtUtc"
            FROM "order".order_status_history
            WHERE order_id = @Id
            ORDER BY changed_at_utc;
            """;

        using IDbConnection connection = connectionFactory.CreateConnection();

        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);

        var order = await multi.ReadSingleOrDefaultAsync<OrderRow>();

        if (order is null)
        {
            return null;
        }

        var items = (await multi.ReadAsync<OrderItemDto>()).ToList();
        var statusHistory = (await multi.ReadAsync<StatusHistoryRow>())
            .Select(h => new OrderStatusHistoryDto(h.Status.ToString(), h.Note, h.ChangedAtUtc))
            .ToList();

        return new OrderDetailDto(
            order.Id,
            order.OrderNumber,
            order.UserId,
            order.Status.ToString(),
            order.ShippingRecipientName,
            order.ShippingPhoneNumber,
            order.ShippingCity,
            order.ShippingDistrict,
            order.ShippingFullAddressLine,
            order.ShippingPostalCode,
            order.CouponCode,
            order.DiscountAmount,
            order.CreatedAtUtc,
            items,
            statusHistory,
            Math.Max(0, items.Sum(i => i.LineTotal) - order.DiscountAmount));
    }

    public async Task<PagedResult<OrderSummaryDto>> GetByUserIdAsync(
        Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT COUNT(*) FROM "order".orders WHERE user_id = @UserId;

            SELECT
                o.id AS "Id",
                o.order_number AS "OrderNumber",
                o.status AS "Status",
                GREATEST(0, (SELECT COALESCE(SUM(oi.unit_price * oi.quantity), 0) FROM "order".order_items oi WHERE oi.order_id = o.id) - o.discount_amount) AS "Total",
                o.created_at_utc AS "CreatedAtUtc"
            FROM "order".orders o
            WHERE o.user_id = @UserId
            ORDER BY o.created_at_utc DESC
            LIMIT @PageSize OFFSET @Offset;
            """;

        using IDbConnection connection = connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            sql,
            new { UserId = userId, PageSize = pageSize, Offset = (pageNumber - 1) * pageSize },
            cancellationToken: cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);

        var totalCount = await multi.ReadSingleAsync<int>();
        var rows = (await multi.ReadAsync<OrderSummaryRow>()).ToList();

        var items = rows
            .Select(r => new OrderSummaryDto(r.Id, r.OrderNumber, r.Status.ToString(), r.Total, r.CreatedAtUtc))
            .ToList();

        return new PagedResult<OrderSummaryDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
        };
    }

    public async Task<PagedResult<OrderSummaryDto>> GetAllAsync(
        OrderStatus? status, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        // Status is stored as a plain integer column (EF Core's default enum mapping, no
        // HasConversion configured in OrderConfiguration) — filtered as an int, not text.
        const string sql = """
            SELECT COUNT(*) FROM "order".orders o
            WHERE @Status::int IS NULL OR o.status = @Status;

            SELECT
                o.id AS "Id",
                o.order_number AS "OrderNumber",
                o.status AS "Status",
                GREATEST(0, (SELECT COALESCE(SUM(oi.unit_price * oi.quantity), 0) FROM "order".order_items oi WHERE oi.order_id = o.id) - o.discount_amount) AS "Total",
                o.created_at_utc AS "CreatedAtUtc"
            FROM "order".orders o
            WHERE @Status::int IS NULL OR o.status = @Status
            ORDER BY o.created_at_utc DESC
            LIMIT @PageSize OFFSET @Offset;
            """;

        using IDbConnection connection = connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            sql,
            new { Status = (int?)status, PageSize = pageSize, Offset = (pageNumber - 1) * pageSize },
            cancellationToken: cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);

        var totalCount = await multi.ReadSingleAsync<int>();
        var rows = (await multi.ReadAsync<OrderSummaryRow>()).ToList();

        var items = rows
            .Select(r => new OrderSummaryDto(r.Id, r.OrderNumber, r.Status.ToString(), r.Total, r.CreatedAtUtc))
            .ToList();

        return new PagedResult<OrderSummaryDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
        };
    }
}
