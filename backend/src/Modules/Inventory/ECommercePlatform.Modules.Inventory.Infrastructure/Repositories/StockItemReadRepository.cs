using System.Data;
using Dapper;
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
}
