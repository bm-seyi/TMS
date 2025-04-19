using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using TMS_API.Hubs;
using TMS_API.Models.Data;
using TMS_API.Utilities;

namespace TMS_API.Tests.Hubs
{
    [TestClass]
    public class LinesHubTests
    {
        private Mock<ILogger<LinesHub>> _mockLogger = null!;
        private Mock<IDatabaseActions> _mockDatabaseActions = null!;
        private Mock<IHubCallerClients> _mockClients = null!;
        private Mock<ISingleClientProxy> _mockClientProxy =  null!;
        private Mock<HubCallerContext> _mockContext = null!;
        private LinesHub _linesHub = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<LinesHub>>();
            _mockDatabaseActions = new  Mock<IDatabaseActions>();
            _mockClients = new Mock<IHubCallerClients>();
            _mockClientProxy = new Mock<ISingleClientProxy>();
            _mockContext = new Mock<HubCallerContext>();

            // Setup default mock behaviors
            _mockContext.SetupGet(c => c.ConnectionId).Returns("test-connection-id");
            _mockClients.Setup(c => c.Caller).Returns(_mockClientProxy.Object);
            
            _linesHub = new LinesHub(_mockLogger.Object, _mockDatabaseActions.Object)
            {
                Clients = _mockClients.Object,
                Context = _mockContext.Object
            };
        }

        [TestMethod]
        public void Constructor_ShouldThrowArgumentNullException_WhenDependenciesAreNull()
        {
            // Arrange & Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new LinesHub(_mockLogger.Object, null!));
            Assert.ThrowsException<ArgumentNullException>(() => new LinesHub(null!, _mockDatabaseActions.Object));
        }

        [TestMethod]
        public async Task OnConnectedAsync_ShouldRetrieveAndSendLinesData()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var expectedLinesData = new List<LinesModel>
            {
                new LinesModel { Id = Guid.NewGuid(), Latitude = 1.0, Longitude = 2.0 }
            };

            _mockDatabaseActions.Setup(d => d.RetrieveModelAsync<LinesModel>(It.IsAny<String>(),cancellationToken))
                .ReturnsAsync(expectedLinesData)
                .Verifiable();

            _mockClientProxy.Setup(x => x.SendCoreAsync("ReceiveLinesData", It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);


            // Act
            await _linesHub.OnConnectedAsync();

            // Assert
            _mockDatabaseActions.Verify();
            _mockClientProxy.Verify();
            
            _mockLogger.Verify(
            logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Client connected to LinesHub: test-connection-id"),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
                Times.Once);

            _mockLogger.Verify(
            logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Lines data sent to client: test-connection-id"),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
                Times.Once);
            
            _mockClientProxy.Verify(
                proxy => proxy.SendCoreAsync("ReceiveLinesData", It.IsAny<object[]>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }

}