using Microsoft.AspNetCore.SignalR;
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

        [TestMethod]
        public async Task ExecuteAsync_WhenProcessingLinesModel_SendsDataAndLogsInfo()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            var processed = new TaskCompletionSource<bool>();

            var service = new LinesBackgroundService(_mockHubContext.Object, _mockLogger.Object, _mockServiceScopeFactory.Object);

            Mock<IServiceScope> mockServiceScope = new Mock<IServiceScope>();
            _mockServiceScopeFactory.Setup(x => x.CreateScope()).Returns(mockServiceScope.Object);
            
            Mock<IDatabaseActions> mockDatabaseActions = new Mock<IDatabaseActions>();
            
            mockDatabaseActions.Setup(x => x.RetrieveModelAsync<LinesQueueModel>(It.IsAny<string>(), It.IsAny<bool>(),It.IsAny<CancellationToken>()))
                .Returns(async (string test, bool value,CancellationToken token) => 
                {
                    await Task.Delay(100, token);
                    processed.SetResult(true);
                    return new List<LinesQueueModel> { new LinesQueueModel { Id = Guid.NewGuid(), Latitude = 1.0, Longitude = 2.0 } };
                });

            mockServiceScope.Setup(x => x.ServiceProvider.GetService(typeof(IDatabaseActions)))
                .Returns(mockDatabaseActions.Object);

            Mock<IClientProxy> mockClientProxy = new Mock<IClientProxy>();
            _mockHubContext.Setup(x => x.Clients.All).Returns(mockClientProxy.Object);

            mockClientProxy.Setup(x => x.SendCoreAsync("ReceiveLinesData", It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);


            // Act
            var task = service.StartAsync(cts.Token);
            await Task.WhenAny(processed.Task, Task.Delay(1000));
            cts.Cancel();
            await service.StopAsync(CancellationToken.None);
            await Task.Delay(100);

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

            _mockLogger.Verify(
            logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Lines data sent to all clients."),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
                Times.AtLeastOnce);
            
            mockClientProxy.Verify(
                proxy => proxy.SendCoreAsync("ReceiveLinesData", It.IsAny<object[]>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }



        [TestMethod]
        public async Task ExecuteAsync_OperationCancelled_SendsDataAndLogsInfo()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            var processed = new TaskCompletionSource<bool>();

            var service = new LinesBackgroundService(_mockHubContext.Object, _mockLogger.Object, _mockServiceScopeFactory.Object);

            Mock<IServiceScope> mockServiceScope = new Mock<IServiceScope>();
            _mockServiceScopeFactory.Setup(x => x.CreateScope()).Returns(mockServiceScope.Object);
            
            Mock<IDatabaseActions> mockDatabaseActions = new Mock<IDatabaseActions>();
            
            mockDatabaseActions.Setup(x => x.RetrieveModelAsync<LinesQueueModel>(It.IsAny<string>(), It.IsAny<bool>(),It.IsAny<CancellationToken>()))
                .Returns(async (string test, bool value ,CancellationToken token) => 
                {
                    await Task.Delay(100, token);
                    processed.SetResult(true);
                    return new List<LinesQueueModel>();
                });

            mockServiceScope.Setup(x => x.ServiceProvider.GetService(typeof(IDatabaseActions)))
                .Returns(mockDatabaseActions.Object);
            
            Mock<IClientProxy> mockClientProxy = new Mock<IClientProxy>();
            _mockHubContext.Setup(x => x.Clients.All).Returns(mockClientProxy.Object);

            mockClientProxy.Setup(x => x.SendCoreAsync("ReceiveLinesData", It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);


            // Act
            var task = service.StartAsync(cts.Token);
            await Task.WhenAny(processed.Task, Task.Delay(1000));
            cts.Cancel();
            await service.StopAsync(CancellationToken.None);
            await Task.Delay(100);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "No data in queue. Waiting for new data..."),
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

            _mockLogger.Verify(
            logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "LinesBackgroundService is stopping due to cancellation."),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
                Times.AtLeastOnce);
            
        }

        [TestMethod]
        public async Task ExecuteAsync_Exception_SendsDataAndLogsInfo()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            var exceptionThrown = new TaskCompletionSource<bool>();

            var service = new LinesBackgroundService(_mockHubContext.Object, _mockLogger.Object, _mockServiceScopeFactory.Object);

            Mock<IServiceScope> mockServiceScope = new Mock<IServiceScope>();
            _mockServiceScopeFactory.Setup(x => x.CreateScope()).Returns(mockServiceScope.Object);
            
            Mock<IDatabaseActions> mockDatabaseActions = new Mock<IDatabaseActions>();
            
            mockDatabaseActions.Setup(x => x.RetrieveModelAsync<LinesQueueModel>(It.IsAny<string>(), It.IsAny<bool>(),It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"))
                .Callback(() => exceptionThrown.SetResult(true));

            mockServiceScope.Setup(x => x.ServiceProvider.GetService(typeof(IDatabaseActions)))
                .Returns(mockDatabaseActions.Object);

            // Act
            var task = service.StartAsync(cts.Token);
            await exceptionThrown.Task;
            cts.Cancel();
            await service.StopAsync(CancellationToken.None);

            await Task.Delay(100);

            // Assert
            _mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Error in Service Broker listener: Test exception"),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }
    }

}