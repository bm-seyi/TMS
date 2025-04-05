using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TMS_API.Hubs;
using TMS_API.Models.Data;
using TMS_API.Utilities;

namespace TMS_API.Tests.BackgroundServices
{
    [TestClass]
    public class LinesBackgroundServiceTests
    {
        private Mock<ILogger<LinesBackgroundService>> _mockLogger = null!;
        private Mock<IHubContext<LinesHub>> _mockHubContext = null!;
        private Mock<IServiceScopeFactory> _mockServiceScopeFactory = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<LinesBackgroundService>>();
            _mockHubContext = new Mock<IHubContext<LinesHub>>();
            _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        }

        [TestMethod]
        public void Constructor_ShouldThrowArgumentNullException_WhenDependenciesAreNull()
        {
            // Arrange & Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new LinesBackgroundService(null!, _mockLogger.Object, _mockServiceScopeFactory.Object));
            Assert.ThrowsException<ArgumentNullException>(() => new LinesBackgroundService(_mockHubContext.Object, null!, _mockServiceScopeFactory.Object));
            Assert.ThrowsException<ArgumentNullException>(() => new LinesBackgroundService(_mockHubContext.Object, _mockLogger.Object, null!));       
        }

        [TestMethod]
        public async Task ExecuteAsync_WhenStartedAndStopped_LogsStartingAndStopping()
        {
            // Arrange
            var cts = new CancellationTokenSource();

            var service = new LinesBackgroundService(_mockHubContext.Object, _mockLogger.Object, _mockServiceScopeFactory.Object);

            // Act
            var task = service.StartAsync(cts.Token);
            await task;
            cts.Cancel();

            // Assert
            _mockLogger.Verify(
               logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "LinesBackgroundService is starting."),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
                Times.Once);

            _mockLogger.Verify(
               logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "LinesBackgroundService is stopping."),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
                Times.Once);
        }
    }

}