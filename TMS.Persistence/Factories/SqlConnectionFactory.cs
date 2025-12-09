using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TMS.Core.Interfaces.Factories;


namespace TMS.Persistence.Factories
{
    /// <summary>
    /// Handles Database connection creation
    /// </summary>
    internal class SqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SqlConnectionFactory> _logger;
        private static readonly ActivitySource _activitySource = new ActivitySource("TMS.Core.Factories.SqlConnectionFactory");

        public SqlConnectionFactory(IConfiguration configuration, ILogger<SqlConnectionFactory> logger)
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
            using Activity? activity = _activitySource.StartActivity("CreateConnection");
            
            string connectionString = _configuration[$"ConnectionStrings:{connectionName}"] ?? throw new InvalidOperationException($"Unable to retrieve {connectionName} connection string from configuration.");
            _logger.LogInformation("Connection String: {0} was successfully retrieved from the configuration", connectionName);

            return new SqlConnection(connectionString);
        }
    }
}

