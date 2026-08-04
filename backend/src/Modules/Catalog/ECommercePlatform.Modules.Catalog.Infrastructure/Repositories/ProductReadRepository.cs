using System.Data;
using Dapper;
using ECommercePlatform.BuildingBlocks.Application.Models;
using ECommercePlatform.BuildingBlocks.Infrastructure.Persistence;
using ECommercePlatform.Modules.Catalog.Application.Abstractions;
using ECommercePlatform.Modules.Catalog.Application.Dtos;

namespace ECommercePlatform.Modules.Catalog.Infrastructure.Repositories;

internal sealed class ProductReadRepository(ISqlConnectionFactory connectionFactory) : IProductReadRepository
{
    private sealed record ProductRow(
        Guid Id, Guid CategoryId, Guid? BrandId, string Name, string Description, bool IsActive, DateTime CreatedAtUtc);

    private sealed record VariantRow(Guid Id, string Sku, decimal Price, bool IsActive);

    private sealed record AttributeValueRow(
        Guid ProductVariantId, Guid ProductAttributeId, string ProductAttributeName, string Value);

    public async Task<ProductDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                id AS "Id", category_id AS "CategoryId", brand_id AS "BrandId",
                name AS "Name", description AS "Description", is_active AS "IsActive", created_at_utc AS "CreatedAtUtc"
            FROM catalog.products
            WHERE id = @Id;

            SELECT id AS "Id", sku AS "Sku", price AS "Price", is_active AS "IsActive"
            FROM catalog.product_variants
            WHERE product_id = @Id;

            SELECT
                pvav.product_variant_id AS "ProductVariantId",
                pvav.product_attribute_id AS "ProductAttributeId",
                pa.name AS "ProductAttributeName",
                pvav.value AS "Value"
            FROM catalog.product_variant_attribute_values pvav
            JOIN catalog.product_variants pv ON pv.id = pvav.product_variant_id
            JOIN catalog.product_attributes pa ON pa.id = pvav.product_attribute_id
            WHERE pv.product_id = @Id;

            SELECT id AS "Id", url AS "Url", is_primary AS "IsPrimary", display_order AS "DisplayOrder"
            FROM catalog.product_images
            WHERE product_id = @Id
            ORDER BY display_order;
            """;

        using IDbConnection connection = connectionFactory.CreateConnection();

        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);

        var product = await multi.ReadSingleOrDefaultAsync<ProductRow>();

        if (product is null)
        {
            return null;
        }

        var variantRows = (await multi.ReadAsync<VariantRow>()).ToList();
        var attributeValueRows = (await multi.ReadAsync<AttributeValueRow>()).ToList();
        var images = (await multi.ReadAsync<ProductImageDto>()).ToList();

        var variants = variantRows
            .Select(v => new ProductVariantDto(
                v.Id,
                v.Sku,
                v.Price,
                v.IsActive,
                attributeValueRows
                    .Where(a => a.ProductVariantId == v.Id)
                    .Select(a => new ProductVariantAttributeValueDto(a.ProductAttributeId, a.ProductAttributeName, a.Value))
                    .ToList()))
            .ToList();

        return new ProductDetailDto(
            product.Id,
            product.CategoryId,
            product.BrandId,
            product.Name,
            product.Description,
            product.IsActive,
            product.CreatedAtUtc,
            variants,
            images);
    }

    public async Task<PagedResult<ProductSummaryDto>> GetByCategoryIdAsync(
        Guid categoryId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT COUNT(*) FROM catalog.products WHERE category_id = @CategoryId;

            SELECT
                p.id AS "Id",
                p.name AS "Name",
                p.category_id AS "CategoryId",
                p.brand_id AS "BrandId",
                (SELECT MIN(pv.price) FROM catalog.product_variants pv WHERE pv.product_id = p.id) AS "MinPrice",
                (SELECT pi.url FROM catalog.product_images pi WHERE pi.product_id = p.id AND pi.is_primary = true LIMIT 1) AS "PrimaryImageUrl",
                p.is_active AS "IsActive"
            FROM catalog.products p
            WHERE p.category_id = @CategoryId
            ORDER BY p.created_at_utc DESC
            LIMIT @PageSize OFFSET @Offset;
            """;

        using IDbConnection connection = connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            sql,
            new { CategoryId = categoryId, PageSize = pageSize, Offset = (pageNumber - 1) * pageSize },
            cancellationToken: cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);

        var totalCount = await multi.ReadSingleAsync<int>();
        var items = (await multi.ReadAsync<ProductSummaryDto>()).ToList();

        return new PagedResult<ProductSummaryDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
        };
    }

    public async Task<IReadOnlyList<ProductVariantSummaryDto>> GetVariantSummariesAsync(
        IReadOnlyList<Guid> productVariantIds, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                pv.id AS "ProductVariantId",
                p.name AS "ProductName",
                pv.sku AS "Sku",
                pv.price AS "Price",
                (SELECT pi.url FROM catalog.product_images pi WHERE pi.product_id = p.id AND pi.is_primary = true LIMIT 1) AS "ImageUrl",
                pv.is_active AS "IsActive",
                p.id AS "ProductId"
            FROM catalog.product_variants pv
            JOIN catalog.products p ON p.id = pv.product_id
            WHERE pv.id = ANY(@ProductVariantIds);
            """;

        using IDbConnection connection = connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            sql,
            new { ProductVariantIds = productVariantIds.ToArray() },
            cancellationToken: cancellationToken);

        var result = await connection.QueryAsync<ProductVariantSummaryDto>(command);

        return result.ToList();
    }
}
