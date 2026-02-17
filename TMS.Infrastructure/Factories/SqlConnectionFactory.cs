using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TMS.Core.Interfaces.Factories;
using System.Runtime.CompilerServices;
using TMS.Core.Extensions;


[assembly: InternalsVisibleTo("TMS.UnitTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]
namespace TMS.Infrastructure.Factories
{
    /// <summary>
    /// Handles Database connection creation
    /// </summary>
    internal class SqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SqlConnectionFactory> _logger;
        private static readonly ActivitySource _activitySource = new ActivitySource("TMS.Persistence.Factories.SqlConnectionFactory");

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
            using Activity? activity = _activitySource.StartActivity("SqlConnectionFactory.CreateConnection");
            
            string connectionString = _configuration.GetRequiredValue<string>($"ConnectionStrings:{connectionName}");
            _logger.LogInformation("Connection String: {0} was successfully retrieved from the configuration", connectionName);

            return new SqlConnection(connectionString);
        }
    }
}

