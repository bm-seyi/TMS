using Microsoft.Data.SqlClient;

namespace TMS.Core.Interfaces.Factories
{
    public interface ISqlDatabaseFactory
    {
        SqlConnection CreateConnection(string connectionName);
    }
}