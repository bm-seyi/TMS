using Microsoft.Data.SqlClient;

namespace TMS_API.Utilities
{
    public interface IDatabaseActions
    {
        Task<List<Dictionary<string, object>>> RetrieveLinesData();
    }

    public class DatabaseActions : IDatabaseActions
    {
        private readonly string _connectionString;
        private readonly ILogger<DatabaseActions> _logger;
        public DatabaseActions(IConfiguration configuration, ILogger<DatabaseActions> logger)
        {
            _connectionString = configuration["ConnectionStrings:Development"] ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<Dictionary<string, object>>> RetrieveLinesData()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();

                using (SqlCommand command = new SqlCommand("SELECT * FROM [dbo].[Lines]", connection))
                {
                    await using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Dictionary<string, object> row = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row[reader.GetName(i)] = await reader.IsDBNullAsync(i) ? null! : reader.GetValue(i);
                            }
                            rows.Add(row);
                        }
                    }
                }

                return rows;
            }
        }
    }
}