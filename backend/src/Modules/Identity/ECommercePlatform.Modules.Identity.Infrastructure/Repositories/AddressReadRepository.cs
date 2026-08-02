using System.Data;
using Dapper;
using ECommercePlatform.BuildingBlocks.Infrastructure.Persistence;
using ECommercePlatform.Modules.Identity.Application.Abstractions;
using ECommercePlatform.Modules.Identity.Application.Dtos;

namespace ECommercePlatform.Modules.Identity.Infrastructure.Repositories;

internal sealed class AddressReadRepository(ISqlConnectionFactory connectionFactory) : IAddressReadRepository
{
    public async Task<IReadOnlyList<AddressDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                id AS "Id",
                title AS "Title",
                recipient_name AS "RecipientName",
                phone_number AS "PhoneNumber",
                city AS "City",
                district AS "District",
                full_address_line AS "FullAddressLine",
                postal_code AS "PostalCode",
                is_default AS "IsDefault",
                created_at_utc AS "CreatedAtUtc"
            FROM identity.addresses
            WHERE user_id = @UserId
            ORDER BY is_default DESC, created_at_utc DESC
            """;

        using IDbConnection connection = connectionFactory.CreateConnection();

        var command = new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken);

        var addresses = await connection.QueryAsync<AddressDto>(command);

        return addresses.ToList();
    }
}
