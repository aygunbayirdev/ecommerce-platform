using System.Data;
using Dapper;
using ECommercePlatform.BuildingBlocks.Application.Models;
using ECommercePlatform.BuildingBlocks.Infrastructure.Persistence;
using ECommercePlatform.Modules.Promotion.Application.Abstractions;
using ECommercePlatform.Modules.Promotion.Application.Dtos;

namespace ECommercePlatform.Modules.Promotion.Infrastructure.Repositories;

internal sealed class CouponReadRepository(ISqlConnectionFactory connectionFactory) : ICouponReadRepository
{
    public async Task<PagedResult<CouponDto>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT COUNT(*) FROM promotion.coupons;

            SELECT
                c.id AS "Id",
                c.code AS "Code",
                c.discount_type AS "DiscountType",
                c.discount_value AS "DiscountValue",
                c.valid_from AS "ValidFrom",
                c.valid_to AS "ValidTo",
                c.usage_limit AS "UsageLimit",
                (SELECT COUNT(*)::int FROM promotion.coupon_redemptions r
                    WHERE r.coupon_id = c.id AND r.released_at_utc IS NULL) AS "UsedCount",
                c.is_active AS "IsActive",
                c.created_at_utc AS "CreatedAtUtc"
            FROM promotion.coupons c
            ORDER BY c.created_at_utc DESC
            LIMIT @PageSize OFFSET @Offset;
            """;

        using IDbConnection connection = connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            sql,
            new { PageSize = pageSize, Offset = (pageNumber - 1) * pageSize },
            cancellationToken: cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);

        var totalCount = await multi.ReadSingleAsync<int>();
        var items = (await multi.ReadAsync<CouponDto>()).ToList();

        return new PagedResult<CouponDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
        };
    }
}
