using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Xml.Linq;
using TMS_API.Models.Data;

namespace TMS_API.Utilities
{
    public interface IDatabaseActions
    {
        Task<List<T>> RetrieveModelAsync<T>(string query, CancellationToken cancellationToken = default) where T : new();
        Task<object?> ProcessQueueMessageAsync(CancellationToken cancellationToken = default);
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
        
        public async Task<List<T>> RetrieveModelAsync<T>(string query, CancellationToken cancellationToken = default) where T : new()
        {
            await _databaseConnection.OpenAsync(cancellationToken);
            _logger.LogInformation("Connection to database opened");
            
            List<T> listData = new List<T>();

            await using (SqlCommand command = _databaseConnection.CreateSqlCommand(query))
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

                        var value = reader.GetValue(ordinal);
                        prop.SetValue(instance, Convert.ChangeType(value, prop.PropertyType));

                    }
                    listData.Add(instance);
                }
                _logger.LogInformation("Data retrieved and mapped to model: {0}", typeof(T).Name);
            }
            return listData;
        }

        public async Task<object?> ProcessQueueMessageAsync(CancellationToken cancellationToken = default)
        {
        
            await _databaseConnection.OpenAsync(cancellationToken);
            _logger.LogInformation("Connection to database opened for processing queue message");

            List<Guid> affectedIds = new List<Guid>();
            using (SqlCommand command = _databaseConnection.CreateSqlCommand("WAITFOR (RECEIVE TOP (1) conversation_handle, message_body FROM DataChangeQueue), TIMEOUT 10000;"))
            await using (SqlDataReader reader = await _databaseConnection.ExecuteReaderAsync(command, cancellationToken))
            {
                if (await reader.ReadAsync(cancellationToken))
                {
                    Guid conversationHandle = reader.GetGuid(0);
                    _logger.LogInformation("Conversation handle retrieved: {0}", conversationHandle);
                    string? message = await reader.IsDBNullAsync(1, cancellationToken) ? null : reader.GetString(1);
                    List<LinesModel> linesData = new List<LinesModel>();

                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        XDocument xDocument = XDocument.Parse(message);
                        affectedIds.AddRange(
                        xDocument.Descendants("AffectedID")
                                .Select(e => Guid.TryParse(e.Value, out var id) ? id : (Guid?)null)
                                .OfType<Guid>()
                        );
                        _logger.LogInformation("Affected IDs retrieved: {0}", string.Join(", ", affectedIds));

                        if (xDocument.Descendants("OperationType").FirstOrDefault()?.Value == "DELETE")
                        {
                            _logger.LogInformation("Delete operation detected, affected IDs: {0}", string.Join(", ", affectedIds));
                            return affectedIds;
                        }

                        await using (SqlCommand recordsCommand = _databaseConnection.CreateSqlCommand("SELECT [Id], [Latitude], [Longitude] FROM [dbo].[Lines] WHERE [ID] IN (@ids)"))
                        {
                            recordsCommand.Parameters.AddWithValue("@ids", string.Join(",", affectedIds));
                            linesData = await RetrieveModelAsync<LinesModel>(recordsCommand.CommandText, cancellationToken);
                            _logger.LogInformation("Lines data retrieved for affected IDs: {0}", string.Join(", ", affectedIds));
                        }

                    }

                    using (SqlCommand endConversation = _databaseConnection.CreateSqlCommand("END CONVERSATION @conv;"))
                    {
                        endConversation.Parameters.AddWithValue("@conv", conversationHandle);
                        await endConversation.ExecuteNonQueryAsync(cancellationToken);
                        _logger.LogInformation("Conversation ended: {0}", conversationHandle);
                    }
                    
                    return linesData;
                }
            }
            _logger.LogInformation("No messages in queue");
            return null;
        }
    }
}
