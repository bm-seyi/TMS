using Microsoft.Data.SqlClient;

namespace TMS.Application.Interfaces.Factories
{
    public interface ISqlConnectionFactory
    {
        SqlConnection CreateConnection(string connectionName);
    }
}