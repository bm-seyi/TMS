using Microsoft.Data.SqlClient;

namespace Core.Interfaces.Factories
{
    public interface ISqlDatabaseFactory
    {
        SqlConnection CreateConnection(string connectionName);
    }
}