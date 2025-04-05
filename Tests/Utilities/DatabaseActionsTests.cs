using System.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TMS_API.Hubs;
using TMS_API.Models.Data;
using TMS_API.Utilities;


namespace TMS_API.Tests.Utilities
{
    [TestClass]
    public class DatabaseActionsTests
    {
        private Mock<ILogger<DatabaseActions>> _mockLogger = null!;
        private Mock<IDatabaseConnection> _mockDatabaseConnection = null!;
     

        [TestInitialize]
        public void Setup()
        {
           _mockLogger = new Mock<ILogger<DatabaseActions>>();
           _mockDatabaseConnection = new Mock<IDatabaseConnection>();

        }

        [TestMethod]
        public void Constructor_ShouldThrowArgumentNullException_WhenDependenciesAreNull()
        {
            // Arrange & Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new DatabaseActions(null!, _mockDatabaseConnection.Object));
            Assert.ThrowsException<ArgumentNullException>(() => new DatabaseActions(_mockLogger.Object, null!));
        }

    }
}