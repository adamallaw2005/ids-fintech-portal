using System.Data;
using Microsoft.Data.SqlClient;

namespace MyWebsite_API.Data;

public sealed class SqlConnectionFactory(IConfiguration configuration) : IDbConnectionFactory
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}
