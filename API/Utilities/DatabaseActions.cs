using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace TMS_API.Utilities
{
    public interface IDatabaseActions
    {
        Task<List<T>> RetrieveModelAsync<T>(string query, bool isStoredProcedure, CancellationToken cancellationToken = default) where T : new();
    }

    [ExcludeFromCodeCoverage]
    public class DatabaseActions : IDatabaseActions
    {
        private readonly ILogger<DatabaseActions> _logger;
        private readonly IDatabaseConnection _databaseConnection;
        public DatabaseActions(ILogger<DatabaseActions> logger, IDatabaseConnection databaseConnection)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseConnection = databaseConnection ?? throw new ArgumentNullException(nameof(databaseConnection));
        }

        public async Task<List<T>> RetrieveModelAsync<T>(string query, bool isStoredProcedure, CancellationToken cancellationToken = default) where T : new()
        {
            await _databaseConnection.OpenAsync(cancellationToken);
            _logger.LogInformation("Connection to database opened");

            List<T> listData = new List<T>();

            await using (SqlCommand command = _databaseConnection.CreateSqlCommand(query))
            {
                command.CommandType = isStoredProcedure ? CommandType.StoredProcedure : CommandType.Text;
                await using (SqlDataReader reader = await _databaseConnection.ExecuteReaderAsync(command, cancellationToken))
                {
                    var columnOrdinals = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    for (int i = 0; i < reader.FieldCount; i++)
                        columnOrdinals[reader.GetName(i)] = i;
                    _logger.LogInformation("Column ordinals retrieved");

                    var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                    while (await reader.ReadAsync(cancellationToken))
                    {
                        var instance = new T();

                        foreach (var prop in properties)
                        {
                            if (!columnOrdinals.TryGetValue(prop.Name, out int ordinal) || await reader.IsDBNullAsync(ordinal))
                                continue;

                            object value = reader.GetValue(ordinal);
                            Type propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                            prop.SetValue(instance, Convert.ChangeType(value, propType));

                        }
                        listData.Add(instance);
                    }
                    _logger.LogInformation("Data retrieved and mapped to model: {0}", typeof(T).Name);
                }
            }
            return listData;
        }
    }
}