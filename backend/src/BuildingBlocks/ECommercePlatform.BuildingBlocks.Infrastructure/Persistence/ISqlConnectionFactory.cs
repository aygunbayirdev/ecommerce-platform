using System.Data;

namespace ECommercePlatform.BuildingBlocks.Infrastructure.Persistence;

public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}
