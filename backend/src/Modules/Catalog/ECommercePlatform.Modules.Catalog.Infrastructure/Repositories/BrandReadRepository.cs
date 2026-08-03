using System.Data;
using Dapper;
using ECommercePlatform.BuildingBlocks.Infrastructure.Persistence;
using ECommercePlatform.Modules.Catalog.Application.Abstractions;
using ECommercePlatform.Modules.Catalog.Application.Dtos;

namespace ECommercePlatform.Modules.Catalog.Infrastructure.Repositories;

internal sealed class BrandReadRepository(ISqlConnectionFactory connectionFactory) : IBrandReadRepository
{
    public async Task<IReadOnlyList<BrandDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                id AS "Id",
                name AS "Name",
                is_active AS "IsActive",
                created_at_utc AS "CreatedAtUtc"
            FROM catalog.brands
            ORDER BY name
            """;

        using IDbConnection connection = connectionFactory.CreateConnection();

        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);

        var brands = await connection.QueryAsync<BrandDto>(command);

        return brands.ToList();
    }
}
