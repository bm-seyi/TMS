using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Core.Interfaces.Factories;


namespace Core.Factories
{
    /// <summary>
    /// Handles Database connection creation
    /// </summary>
    public class SqlDatabaseFactory : ISqlDatabaseFactory
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SqlDatabaseFactory> _logger;

        public SqlDatabaseFactory(IConfiguration configuration, ILogger<SqlDatabaseFactory> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves connection string from configuration and creates SqlConnection Object
        /// </summary>
        /// <param name="connectionName">The name within 'ConnectionStrings' section in the configuration</param>
        /// <returns>An SqlConnection Object</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public SqlConnection CreateConnection(string connectionName)
        {
            string connectionString = _configuration[$"ConnectionStrings:{connectionName}"] ?? throw new InvalidOperationException($"Unable to retrieve {connectionName} connection string from configuration.");
            _logger.LogInformation("Connection String: {0} was successfully retrieved from the configuration", connectionName);

            return new SqlConnection(connectionString);
        }
    }
}

