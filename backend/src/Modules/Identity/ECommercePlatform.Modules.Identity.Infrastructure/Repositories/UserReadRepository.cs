using System.Data;
using Dapper;
using ECommercePlatform.BuildingBlocks.Infrastructure.Persistence;
using ECommercePlatform.Modules.Identity.Application.Abstractions;
using ECommercePlatform.Modules.Identity.Application.Dtos;

namespace ECommercePlatform.Modules.Identity.Infrastructure.Repositories;

internal sealed class UserReadRepository(ISqlConnectionFactory connectionFactory) : IUserReadRepository
{
    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                id AS "Id",
                email AS "Email",
                first_name AS "FirstName",
                last_name AS "LastName",
                phone_number AS "PhoneNumber",
                role AS "Role",
                is_active AS "IsActive",
                created_at_utc AS "CreatedAtUtc"
            FROM identity.users
            WHERE id = @Id
            """;

        using IDbConnection connection = connectionFactory.CreateConnection();

        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<UserDto>(command);
    }
}
