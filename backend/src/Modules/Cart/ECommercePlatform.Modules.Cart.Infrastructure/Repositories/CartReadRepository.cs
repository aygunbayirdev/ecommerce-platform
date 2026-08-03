using System.Data;
using Dapper;
using ECommercePlatform.BuildingBlocks.Infrastructure.Persistence;
using ECommercePlatform.Modules.Cart.Application.Abstractions;
using ECommercePlatform.Modules.Cart.Application.Dtos;

namespace ECommercePlatform.Modules.Cart.Infrastructure.Repositories;

internal sealed class CartReadRepository(ISqlConnectionFactory connectionFactory) : ICartReadRepository
{
    private sealed record CartRow(Guid Id, Guid? UserId);

    public async Task<CartRawDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id AS "Id", user_id AS "UserId"
            FROM cart.carts
            WHERE id = @Id;

            SELECT product_variant_id AS "ProductVariantId", quantity AS "Quantity"
            FROM cart.cart_items
            WHERE cart_id = @Id;
            """;

        using IDbConnection connection = connectionFactory.CreateConnection();

        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);

        var cart = await multi.ReadSingleOrDefaultAsync<CartRow>();

        if (cart is null)
        {
            return null;
        }

        var items = (await multi.ReadAsync<CartRawItemDto>()).ToList();

        return new CartRawDto(cart.Id, cart.UserId, items);
    }
}
