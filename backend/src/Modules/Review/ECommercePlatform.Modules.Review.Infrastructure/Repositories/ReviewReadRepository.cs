using System.Data;
using Dapper;
using ECommercePlatform.BuildingBlocks.Application.Models;
using ECommercePlatform.BuildingBlocks.Infrastructure.Persistence;
using ECommercePlatform.Modules.Review.Application.Abstractions;
using ECommercePlatform.Modules.Review.Application.Dtos;

namespace ECommercePlatform.Modules.Review.Infrastructure.Repositories;

internal sealed class ReviewReadRepository(ISqlConnectionFactory connectionFactory) : IReviewReadRepository
{
    public async Task<PagedResult<ReviewDto>> GetApprovedByProductIdAsync(
        Guid productId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT COUNT(*) FROM review.reviews WHERE product_id = @ProductId AND is_approved = true;

            SELECT
                id AS "Id", product_id AS "ProductId", user_id AS "UserId", order_id AS "OrderId",
                rating AS "Rating", comment AS "Comment", is_approved AS "IsApproved", created_at_utc AS "CreatedAtUtc"
            FROM review.reviews
            WHERE product_id = @ProductId AND is_approved = true
            ORDER BY created_at_utc DESC
            LIMIT @PageSize OFFSET @Offset;
            """;

        return await QueryPagedAsync(sql, new { ProductId = productId, PageSize = pageSize, Offset = (pageNumber - 1) * pageSize }, pageNumber, pageSize, cancellationToken);
    }

    public async Task<PagedResult<ReviewDto>> GetPendingAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT COUNT(*) FROM review.reviews WHERE is_approved = false;

            SELECT
                id AS "Id", product_id AS "ProductId", user_id AS "UserId", order_id AS "OrderId",
                rating AS "Rating", comment AS "Comment", is_approved AS "IsApproved", created_at_utc AS "CreatedAtUtc"
            FROM review.reviews
            WHERE is_approved = false
            ORDER BY created_at_utc
            LIMIT @PageSize OFFSET @Offset;
            """;

        return await QueryPagedAsync(sql, new { PageSize = pageSize, Offset = (pageNumber - 1) * pageSize }, pageNumber, pageSize, cancellationToken);
    }

    private async Task<PagedResult<ReviewDto>> QueryPagedAsync(
        string sql, object parameters, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        using IDbConnection connection = connectionFactory.CreateConnection();

        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);

        var totalCount = await multi.ReadSingleAsync<int>();
        var items = (await multi.ReadAsync<ReviewDto>()).ToList();

        return new PagedResult<ReviewDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
        };
    }
}
