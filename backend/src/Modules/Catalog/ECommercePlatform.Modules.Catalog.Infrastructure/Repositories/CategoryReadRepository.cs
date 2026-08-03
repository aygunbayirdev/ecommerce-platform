using System.Data;
using Dapper;
using ECommercePlatform.BuildingBlocks.Infrastructure.Persistence;
using ECommercePlatform.Modules.Catalog.Application.Abstractions;
using ECommercePlatform.Modules.Catalog.Application.Dtos;

namespace ECommercePlatform.Modules.Catalog.Infrastructure.Repositories;

internal sealed class CategoryReadRepository(ISqlConnectionFactory connectionFactory) : ICategoryReadRepository
{
    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                id AS "Id",
                name AS "Name",
                parent_category_id AS "ParentCategoryId",
                display_order AS "DisplayOrder",
                is_active AS "IsActive",
                created_at_utc AS "CreatedAtUtc"
            FROM catalog.categories
            ORDER BY parent_category_id NULLS FIRST, display_order
            """;

        using IDbConnection connection = connectionFactory.CreateConnection();

        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);

        var categories = await connection.QueryAsync<CategoryDto>(command);

        return categories.ToList();
    }

    public async Task<IReadOnlyList<ProductAttributeDto>> GetAttributesByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                pa.id AS "Id",
                pa.name AS "Name",
                pa.created_at_utc AS "CreatedAtUtc"
            FROM catalog.category_attributes ca
            JOIN catalog.product_attributes pa ON pa.id = ca.product_attribute_id
            WHERE ca.category_id = @CategoryId
            ORDER BY pa.name
            """;

        using IDbConnection connection = connectionFactory.CreateConnection();

        var command = new CommandDefinition(sql, new { CategoryId = categoryId }, cancellationToken: cancellationToken);

        var attributes = await connection.QueryAsync<ProductAttributeDto>(command);

        return attributes.ToList();
    }
}
