using System.Data;
using Dapper;
using ECommercePlatform.BuildingBlocks.Application.Models;
using ECommercePlatform.BuildingBlocks.Infrastructure.Persistence;
using ECommercePlatform.Modules.Inventory.Application.Abstractions;
using ECommercePlatform.Modules.Inventory.Application.Dtos;

namespace ECommercePlatform.Modules.Inventory.Infrastructure.Repositories;

internal sealed class StockItemReadRepository(ISqlConnectionFactory connectionFactory) : IStockItemReadRepository
{
    public async Task<StockItemDto?> GetByProductVariantIdAsync(Guid productVariantId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                id AS "Id",
                product_variant_id AS "ProductVariantId",
                available_quantity AS "AvailableQuantity",
                reserved_quantity AS "ReservedQuantity",
                created_at_utc AS "CreatedAtUtc"
            FROM inventory.stock_items
            WHERE product_variant_id = @ProductVariantId
            """;

        using IDbConnection connection = connectionFactory.CreateConnection();

        var command = new CommandDefinition(sql, new { ProductVariantId = productVariantId }, cancellationToken: cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<StockItemDto>(command);
    }

    public async Task<PagedResult<StockItemDto>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT COUNT(*) FROM inventory.stock_items;

            SELECT
                id AS "Id",
                product_variant_id AS "ProductVariantId",
                available_quantity AS "AvailableQuantity",
                reserved_quantity AS "ReservedQuantity",
                created_at_utc AS "CreatedAtUtc"
            FROM inventory.stock_items
            ORDER BY created_at_utc DESC
            LIMIT @PageSize OFFSET @Offset;
            """;

        using IDbConnection connection = connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            sql,
            new { PageSize = pageSize, Offset = (pageNumber - 1) * pageSize },
            cancellationToken: cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);

        var totalCount = await multi.ReadSingleAsync<int>();
        var items = (await multi.ReadAsync<StockItemDto>()).ToList();

        return new PagedResult<StockItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
        };
    }
}
