using System.Data;

namespace Starter.Api.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
