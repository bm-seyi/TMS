using Microsoft.Data.SqlClient;
using System.Data;


public interface IDatabaseConnection : IAsyncDisposable
{
    Task OpenAsync(CancellationToken cancellationToken = default);
    Task<int> ExecuteNonQueryAsync(SqlCommand command, CancellationToken cancellationToken = default);
    Task<SqlDataReader> ExecuteReaderAsync(SqlCommand command, CancellationToken cancellationToken = default);
    SqlCommand CreateSqlCommand(string query);
    ConnectionState Status();
}

public class DatabaseConnection : IDatabaseConnection
{
    private readonly SqlConnection _connection;
    public DatabaseConnection(IConfiguration configuration)
    {
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));

        _connection = new SqlConnection(configuration["ConnectionStrings:Development"] ?? throw new ArgumentNullException(nameof(configuration)));
    }
    public async Task OpenAsync(CancellationToken cancellationToken = default)
    {
        if (_connection.State != ConnectionState.Open)
        {
            await _connection.OpenAsync(cancellationToken);
        }
    } 

    public SqlCommand CreateSqlCommand(string query)
    {
        if (_connection.State != ConnectionState.Open)
        {
            _connection.Open();
        }

        return new SqlCommand(query, _connection);
    }

    public async Task<int> ExecuteNonQueryAsync(SqlCommand command, CancellationToken cancellationToken = default)
    {
        if (_connection.State != ConnectionState.Open)
        {
            await OpenAsync(cancellationToken);
        }

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<SqlDataReader> ExecuteReaderAsync(SqlCommand command, CancellationToken cancellationToken = default)
    {
        if (_connection.State != ConnectionState.Open)
        {
            await OpenAsync(cancellationToken);
        }
        return await command.ExecuteReaderAsync(CommandBehavior.Default, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection.State != ConnectionState.Closed)
        {
            await _connection.CloseAsync();
        }
        await _connection.DisposeAsync();
    }

    public ConnectionState Status() => _connection.State;
}