using Microsoft.Data.SqlClient;

public interface ISqlDependencyManager
{
    void Start(string connectionString);
    void Stop(string connectionString);
}

public class SqlDependencyManager : ISqlDependencyManager
{
    public void Start(string connectionString) => SqlDependency.Start(connectionString);
    public void Stop(string connectionString) => SqlDependency.Stop(connectionString);
}