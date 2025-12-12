using Microsoft.Data.SqlClient;

namespace TMS.Core.Interfaces.Factories
{
    public interface ISqlConnectionFactory
    {
        SqlConnection CreateConnection(string connectionName);
    }
}