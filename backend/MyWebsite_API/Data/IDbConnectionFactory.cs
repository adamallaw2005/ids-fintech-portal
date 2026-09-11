using System.Data;

namespace MyWebsite_API.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
