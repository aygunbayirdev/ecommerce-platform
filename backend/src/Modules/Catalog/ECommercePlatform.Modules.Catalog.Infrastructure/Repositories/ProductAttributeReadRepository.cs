using System.Data;
using Dapper;
using ECommercePlatform.BuildingBlocks.Infrastructure.Persistence;
using ECommercePlatform.Modules.Catalog.Application.Abstractions;
using ECommercePlatform.Modules.Catalog.Application.Dtos;

namespace ECommercePlatform.Modules.Catalog.Infrastructure.Repositories;

internal sealed class ProductAttributeReadRepository(ISqlConnectionFactory connectionFactory) : IProductAttributeReadRepository
{
    public async Task<IReadOnlyList<ProductAttributeDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                id AS "Id",
                name AS "Name",
                created_at_utc AS "CreatedAtUtc"
            FROM catalog.product_attributes
            ORDER BY name
            """;

        using IDbConnection connection = connectionFactory.CreateConnection();

        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);

        var attributes = await connection.QueryAsync<ProductAttributeDto>(command);

        return attributes.ToList();
    }
}
