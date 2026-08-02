using System.Data;
using Npgsql;

namespace ECommercePlatform.BuildingBlocks.Infrastructure.Persistence;

public sealed class NpgsqlConnectionFactory(string connectionString) : ISqlConnectionFactory
{
    public IDbConnection CreateConnection()
    {
        var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        return connection;
    }
}
