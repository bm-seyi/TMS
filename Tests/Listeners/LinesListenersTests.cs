using System.Data;
using System.Data.Common;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TMS_API.Hubs;
using TMS_API.Listeners;
using TMS_API.Utilities;

namespace TMS_API_Tests.Listeners
{

    [TestClass]
    public class LineListenersTests
    {
        private Mock<ISqlDependencyManager> _mockSqlDependencyManager = null!;
        private Mock<IHubContext<LinesHub>> _mockHubContext = null!;
        private Mock<ILogger<LinesListener>> _mockLogger = null!;
        private Mock<IDatabaseActions> _mockDatabaseActions = null!;
        private Mock<IConfiguration> _mockConfiguration = null!;
        private Mock<IDbConnection> _mockDbConnection = null!;
        private LinesListener? _linesListener;

        [TestInitialize]
        public void Setup()
        {
            _mockSqlDependencyManager = new Mock<ISqlDependencyManager>();
            _mockHubContext = new  Mock<IHubContext<LinesHub>>();
            _mockLogger = new Mock<ILogger<LinesListener>>();
            _mockDatabaseActions = new Mock<IDatabaseActions>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockDbConnection = new Mock<IDbConnection>();

            _mockConfiguration.Setup(c => c["ConnectionStrings:Development"]).Returns("FakeConnectionString");

            
            _linesListener = new LinesListener(
                _mockSqlDependencyManager.Object,
                _mockHubContext.Object,
                _mockConfiguration.Object,
                _mockLogger.Object,
                _mockDatabaseActions.Object,
                _mockDbConnection.Object
            );
            
        }
        
        
        [TestMethod]
        public void Constructor_ShouldThrowArgumentNullException_WhenDependenciesAreNull()
        {
            // Arrange & Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new LinesListener(null!, _mockHubContext.Object, _mockConfiguration.Object, _mockLogger.Object, _mockDatabaseActions.Object, _mockDbConnection.Object));
            Assert.ThrowsException<ArgumentNullException>(() => new LinesListener(_mockSqlDependencyManager.Object, null!, _mockConfiguration.Object, _mockLogger.Object, _mockDatabaseActions.Object, _mockDbConnection.Object));
            Assert.ThrowsException<ArgumentNullException>(() => new LinesListener(_mockSqlDependencyManager.Object, _mockHubContext.Object, null!, _mockLogger.Object, _mockDatabaseActions.Object, _mockDbConnection.Object));
            Assert.ThrowsException<ArgumentNullException>(() => new LinesListener(_mockSqlDependencyManager.Object, _mockHubContext.Object, _mockConfiguration.Object, null!, _mockDatabaseActions.Object, _mockDbConnection.Object));
            Assert.ThrowsException<ArgumentNullException>(() => new LinesListener(_mockSqlDependencyManager.Object, _mockHubContext.Object, _mockConfiguration.Object, _mockLogger.Object, null!, _mockDbConnection.Object));
            Assert.ThrowsException<ArgumentNullException>(() => new LinesListener(_mockSqlDependencyManager.Object, _mockHubContext.Object, _mockConfiguration.Object, _mockLogger.Object, _mockDatabaseActions.Object, null!));
        }
        
        /*
        [TestMethod]
        public async Task StartListening_ShouldSetUpSqlDependency()
        {
            // Arrange
            var mockSqlDependency = new Mock<ISqlDependency>();
            _mockSqlDependencyManager
                .Setup(m => m.Create(It.IsAny<SqlCommand>()))
                .Returns(mockSqlDependency.Object);

            // Act
            await linesListener.StartListening();
        
            // Assert
            _mockSqlDependencyManager.Verify(m => m.Create(It.IsAny<SqlCommand>()), Times.Once);
            mockSqlDependency.VerifySet(d => d.OnChange += It.IsAny<OnChangeEventHandler>(), Times.Once);
        }

         [TestMethod]
        public async Task StopListeningAsync_ShouldCleanUpResources()
        {
            // Arrange
            var mockSqlDependency = new Mock<ISqlDependency>();
            _mockSqlDependencyManager
                .Setup(m => m.Create(It.IsAny<SqlCommand>()))
                .Returns(mockSqlDependency.Object);

            if (_linesListener == null) throw new ArgumentNullException(nameof(_linesListener));
            await _linesListener.StartListening();

            // Act
            await _linesListener.StopListeningAsync();

            // Assert
            mockSqlDependency.VerifySet(d => d.OnChange -= It.IsAny<OnChangeEventHandler>(), Times.Once);
            _mockLogger.Verify(
               logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "SqlDependency has been successfully stopped."),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
                Times.Once);
        }

        [TestMethod]
        public async Task Dependency_OnChange_ShouldCallHandleDependencyChangeAsync()
        {
            // Arrange
            var data = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { { "Key1", "Value1" } }
            };
            _mockDatabaseActions.Setup(d => d.RetrieveLinesData()).ReturnsAsync(data);

            if (_linesListener == null) throw new ArgumentNullException(nameof(_linesListener));
            await _linesListener.StartListening();

            // Act
            var eventArgs = new SqlNotificationEventArgs(
                SqlNotificationType.Change,
                SqlNotificationInfo.Insert,
                SqlNotificationSource.Data);
            
            var mockSqlDependency = new Mock<ISqlDependency>();
            mockSqlDependency.Raise(d => d.OnChange += null, new object(), eventArgs);

            // Assert
            _mockDatabaseActions.Verify(d => d.RetrieveLinesData(), Times.Once);
            _mockHubContext.Verify(h => h.Clients.All.SendAsync("ReceiveMessage", data, default), Times.Once);
        }
        */


        [TestCleanup]
        public void Cleanup()
        {
            _mockLogger.Reset();
            _mockDatabaseActions.Reset();
            _mockConfiguration.Reset();
            _mockSqlDependencyManager.Reset();
            _mockHubContext.Reset();
        }
    }
}