using System.Data;
using Npgsql;

namespace Starter.Api.Data;

public sealed class NpgsqlConnectionFactory(string connectionString) : IDbConnectionFactory
{
    public IDbConnection CreateConnection() => new NpgsqlConnection(connectionString);
}
