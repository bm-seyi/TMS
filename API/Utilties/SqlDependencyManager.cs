using Microsoft.Data.SqlClient;

public interface ISqlDependencyManager
{
    void Start(string connectionString);
    void Stop(string connectionString);
    ISqlDependency Create(SqlCommand sqlCommand);
}

public class SqlDependencyManager : ISqlDependencyManager
{
    public void Start(string connectionString) => SqlDependency.Start(connectionString);
    public void Stop(string connectionString) => SqlDependency.Stop(connectionString);
    public ISqlDependency Create(SqlCommand sqlCommand)
    {
        return new SqlDependencyWrapper(sqlCommand);
    }

}

public interface ISqlDependency
{
    event OnChangeEventHandler OnChange;
}

public class SqlDependencyWrapper : ISqlDependency
{
    private readonly SqlDependency _sqlDependency;

    public SqlDependencyWrapper(SqlCommand sqlCommand)
    {
        _sqlDependency = new SqlDependency(sqlCommand);
    }

    public event OnChangeEventHandler OnChange
    {
        add => _sqlDependency.OnChange += value;
        remove => _sqlDependency.OnChange -= value;
    }
}
