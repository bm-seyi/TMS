using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TMS_API.BackgroundServices;
using TMS_API.Listeners;

namespace TMS_API_Tests.BackgroundServices
{
    [TestClass]
    public class HubBackgroundServiceTests
    {
        private Mock<ILogger<HubsBackgroundService>> _mockLogger = null!;
        private Mock<ILinesListener> _mockLinesListener = null!;
        private Mock<IConfiguration> _mockConfiguration = null!;
        private Mock<ISqlDependencyManager> _mockSqlDependencyManager = null!;


        [TestInitialize]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<HubsBackgroundService>>();
            _mockLinesListener = new Mock<ILinesListener>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockSqlDependencyManager = new Mock<ISqlDependencyManager>();
        }


        [TestMethod]
        public void Constructor_ShouldThrowArgumentNullException_LinesListenerIsNull()
        {
            if (_mockLogger == null) throw new ArgumentNullException(nameof(_mockLogger));
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new HubsBackgroundService(null!, _mockLogger.Object,_mockConfiguration.Object, _mockSqlDependencyManager.Object));
        }

        [TestMethod]
        public void Constructor_ShouldThrowArgumentNullException_ILoggerIsNull()
        {
            if (_mockLogger == null) throw new ArgumentNullException(nameof(_mockLogger));
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new HubsBackgroundService(_mockLinesListener.Object, null!, _mockConfiguration.Object, _mockSqlDependencyManager.Object));
        }

        [TestMethod]
        public void Constructor_ShouldThrowArgumentNullException_WhenConfigurationIsNull()
        {
            if (_mockConfiguration == null) throw new ArgumentNullException(nameof(_mockConfiguration));
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new HubsBackgroundService(_mockLinesListener.Object, _mockLogger.Object, null!, _mockSqlDependencyManager.Object));
        }

        [TestMethod]
        public void Constructor_ShouldThrowArgumentNullException_WhenSqlDependencyManagerIsNull()
        {
            if (_mockConfiguration == null) throw new ArgumentNullException(nameof(_mockConfiguration));
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new HubsBackgroundService(_mockLinesListener.Object, _mockLogger.Object, _mockConfiguration.Object, null!));
        }


        [TestMethod]
        public void Constructor_ShouldThrowArgumentNullException_WhenConnectionStringIsNull()
        {
            Mock<IConfiguration> mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(c => c["ConnectionStrings:Development"]).Returns((string)null!);
            
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new HubsBackgroundService(_mockLinesListener.Object, _mockLogger.Object, mockConfiguration.Object, _mockSqlDependencyManager.Object));
            Assert.AreEqual("_connectionString", exception.ParamName);
        }

        [TestMethod]
        public async Task ExecuteAsync_Should_StartAndStopDependencies()
        {
            CancellationTokenSource CancellationToken = new CancellationTokenSource();
            _mockLinesListener.Setup(x => x.StartListening()).Returns(Task.CompletedTask);
            _mockLinesListener.Setup(x => x.StopListening());

            _mockConfiguration.Setup(c => c["ConnectionStrings:Development"]).Returns("FakeConnectionString");
            _mockSqlDependencyManager.Setup(x => x.Start(It.IsAny<string>())).Verifiable();
            _mockSqlDependencyManager.Setup(x => x.Stop(It.IsAny<string>())).Verifiable();
            

            HubsBackgroundService hubsBackgroundService = new HubsBackgroundService(_mockLinesListener.Object, _mockLogger.Object, _mockConfiguration.Object, _mockSqlDependencyManager.Object);
            await hubsBackgroundService.StartAsync(CancellationToken.Token);
            CancellationToken.Cancel();
            await hubsBackgroundService.StopAsync(CancellationToken.Token);

            // Assert
            _mockSqlDependencyManager.Setup(m => m.Start(It.IsAny<string>())).Verifiable();
            _mockSqlDependencyManager.Setup(m => m.Stop(It.IsAny<string>())).Verifiable();


            _mockLinesListener.Verify(x => x.StartListening(), Times.Once);
            _mockLinesListener.Verify(x => x.StopListening(), Times.Once);
            
            _mockLogger.Verify(
               logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "HubsBackgroundService is starting."),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
                Times.Once);

            _mockLogger.Verify(
               logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "SqlDependency has started."),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
                Times.Once);
            
            _mockLogger.Verify(
               logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "LinesListener has started."),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
                Times.Once);

            _mockLogger.Verify(
               logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "LinesListener has stopped."),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
                Times.Once);

            _mockLogger.Verify(
               logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "SqlDependency has stopped."),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
                Times.Once);
            
            _mockLogger.Verify(
               logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "HubsBackgroundService has stopped."),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
                Times.Once);
        }

        [TestMethod]
        public async Task ExecuteAsync_Should_StartAndStopDependencies_Catch()
        {
            CancellationTokenSource CancellationToken = new CancellationTokenSource();
            _mockLinesListener.Setup(x => x.StartListening()).Returns(Task.CompletedTask);
            _mockLinesListener.Setup(x => x.StopListening());


            Mock<IConfiguration> mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(c => c["ConnectionStrings:Development"]).Returns("FakeConnectionString");
            _mockSqlDependencyManager.Setup(x => x.Start(It.IsAny<string>())).Throws<ArgumentException>();

            HubsBackgroundService hubsBackgroundService = new HubsBackgroundService(_mockLinesListener.Object, _mockLogger.Object, mockConfiguration.Object, _mockSqlDependencyManager.Object);
            await hubsBackgroundService.StartAsync(CancellationToken.Token);
            CancellationToken.Cancel();
            await hubsBackgroundService.StopAsync(CancellationToken.Token);

            // Assert
            _mockLogger.Verify(
               logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "An error occurred in HubsBackgroundService."),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
                Times.Once);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _mockLogger.Reset();
            _mockLinesListener.Reset();
            _mockConfiguration.Reset();
            _mockSqlDependencyManager.Reset();
        }

    }
}