using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using TMS_API.Models.Data;

namespace TMS_API.Utilities
{
    public interface IDatabaseActions
    {
        Task<List<LinesModel>> RetrieveLinesDataAsync(CancellationToken cancellationToken = default);
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

        const string linesQuery = "SELECT [Id], [Latitude], [Longitude] FROM [dbo].[Lines]";

        public async Task<List<LinesModel>> RetrieveLinesDataAsync(CancellationToken cancellationToken = default)
        {
            await _databaseConnection.OpenAsync(cancellationToken);
            _logger.LogInformation("Connection to database opened");
            
            List<LinesModel> LinesData = new List<LinesModel>();

            await using (SqlCommand command = _databaseConnection.CreateSqlCommand(linesQuery))
            await using (SqlDataReader reader = await _databaseConnection.ExecuteReaderAsync(command, cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    LinesModel linesModel = new LinesModel
                    {
                        Id = await reader.IsDBNullAsync(1, cancellationToken) ?  Guid.Empty :  reader.GetGuid(0),
                        Latitude = await reader.IsDBNullAsync(1, cancellationToken) ? 999.9 : (double)reader.GetDecimal(1),
                        Longitude = await reader.IsDBNullAsync(2, cancellationToken) ? 999.9 : (double)reader.GetDecimal(2)
                    };
                    LinesData.Add(linesModel);
                }
            }
            return LinesData;
        }

        public async Task<object?> ProcessQueueMessageAsync(CancellationToken cancellationToken = default)
        {
            await _databaseConnection.OpenAsync(cancellationToken);
            List<Guid> affectedIds = new List<Guid>();
            using (SqlCommand command = _databaseConnection.CreateSqlCommand("WAITFOR (RECEIVE TOP (1) conversation_handle, message_body FROM DataChangeQueue), TIMEOUT 10000;"))
            await using (SqlDataReader reader = await _databaseConnection.ExecuteReaderAsync(command, cancellationToken))
            {
                if (await reader.ReadAsync(cancellationToken))
                {
                    Guid conversationHandle = reader.GetGuid(0);
                    string message = await reader.IsDBNullAsync(1, cancellationToken) ? null! : reader.GetString(1);

                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        XDocument xDocument = XDocument.Parse(message);
                        affectedIds.AddRange(
                        xDocument.Descendants("AffectedID")
                                .Select(e => Guid.TryParse(e.Value, out var id) ? id : (Guid?)null)
                                .OfType<Guid>()
                        );

                        if (xDocument.Descendants("OperationType").FirstOrDefault()?.Value == "DELETE")
                        {
                            return affectedIds;
                        }

                        await using (SqlCommand recordsCommand = _databaseConnection.CreateSqlCommand($"{linesQuery} WHERE [ID] IN (@ids)"))
                        {
                            recordsCommand.Parameters.AddWithValue("@ids", string.Join(",", affectedIds));
                            await using (SqlDataReader recordsReader = await _databaseConnection.ExecuteReaderAsync(recordsCommand, cancellationToken))
                            {
                                List<LinesModel> LinesData = new List<LinesModel>();
                                
                                while (await recordsReader.ReadAsync(cancellationToken))
                                {
                                    LinesModel linesModel = new LinesModel
                                    {
                                        Id = recordsReader.IsDBNull(1) ?  Guid.Empty :  recordsReader.GetGuid(0),
                                        Latitude = recordsReader.IsDBNull(1) ? 999.9 :  (double)recordsReader.GetDecimal(1),
                                        Longitude = recordsReader.IsDBNull(2) ? 999.9 : (double)recordsReader.GetDecimal(2)
                                    };
                                    LinesData.Add(linesModel);
                                }
                                return LinesData;
                            }
                        }
                    }

                    using (SqlCommand endConversation = _databaseConnection.CreateSqlCommand("END CONVERSATION @conv;"))
                    {
                        endConversation.Parameters.AddWithValue("@conv", conversationHandle);
                        await endConversation.ExecuteNonQueryAsync(cancellationToken);
                    }
                }
            }
            return null;
        }
    }
}
