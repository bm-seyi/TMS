using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TMS.API.ExceptionHandlers;


namespace TMS.UnitTests.API.ExceptionHandlers
{
    [TestClass]
    public class OperationCanceledHandlerTests
    {
        private Mock<ILogger<OperationCanceledHandler>> mockLogger = null!;
        private Mock<IProblemDetailsWriter> mockProblemDetailsWriter = null!;
        private Mock<ProblemDetailsFactory> mockProblemDetailsFactory = null!;
        private OperationCanceledHandler _handler = null!;

        [TestInitialize]
        public void Setup()
        {
            mockLogger = new Mock<ILogger<OperationCanceledHandler>>();
            mockProblemDetailsWriter = new Mock<IProblemDetailsWriter>();
            mockProblemDetailsFactory = new Mock<ProblemDetailsFactory>();

            _handler = new OperationCanceledHandler(mockLogger.Object, mockProblemDetailsWriter.Object, mockProblemDetailsFactory.Object);
        }

        [TestMethod]
        public void ConstructorShouldThrowArgumentNullExceptionWhenDependenciesAreNull()
        {
            // Assert
            Assert.Throws<ArgumentNullException>(() => new OperationCanceledHandler(null!, mockProblemDetailsWriter.Object, mockProblemDetailsFactory.Object));
            Assert.Throws<ArgumentNullException>(() => new OperationCanceledHandler(mockLogger.Object, null!, mockProblemDetailsFactory.Object));
            Assert.Throws<ArgumentNullException>(() => new OperationCanceledHandler(mockLogger.Object, mockProblemDetailsWriter.Object, null!));
        }

       
        [TestMethod]
        public async Task TryHandleAsync_WhenOperationCanceledException_ShouldHandle_AndReturnTrue()
        {
            // Arrange
            DefaultHttpContext context = new DefaultHttpContext();
            context.Request.Path = "/cancel-test";
            context.Request.Method = "GET";
            context.TraceIdentifier = "trace-id";

            OperationCanceledException exception = new OperationCanceledException();

            ProblemDetails problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status499ClientClosedRequest,
                Title = "Request Cancelled"
            };

            mockProblemDetailsWriter
                .Setup(x => x.WriteAsync(It.IsAny<ProblemDetailsContext>()))
                .Returns(ValueTask.CompletedTask);

            // Act
            bool result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

            // Assert
            Assert.IsTrue(result);

            mockProblemDetailsWriter.Verify(x => x.WriteAsync(It.IsAny<ProblemDetailsContext>()), Times.Once);
        }

        [TestMethod]
        public async Task TryHandleAsync_WhenNotOperationCanceledException_ShouldReturnFalse()
        {
            // Arrange
            DefaultHttpContext context = new DefaultHttpContext();
            InvalidOperationException exception = new InvalidOperationException();

            // Act
            bool result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

            // Assert
            Assert.IsFalse(result);

            mockProblemDetailsWriter.Verify(x =>
                x.WriteAsync(It.IsAny<ProblemDetailsContext>()),
                Times.Never);
        }

        [TestMethod]
        public async Task TryHandleAsync_WhenOperationCanceledException_ShouldLogError()
        {
            // Arrange
            DefaultHttpContext context = new DefaultHttpContext();
            context.Request.Path = "/cancel-test";
            context.Request.Method = "POST";
            context.TraceIdentifier = "trace-id";

            OperationCanceledException exception = new OperationCanceledException();

            mockProblemDetailsWriter
                .Setup(x => x.WriteAsync(It.IsAny<ProblemDetailsContext>()))
                .Returns(ValueTask.CompletedTask);

            // Act
            await _handler.TryHandleAsync(context, exception, CancellationToken.None);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [TestMethod]
        public async Task TryHandleAsync_ShouldPassCorrectProblemDetailsContext()
        {
            // Arrange
            DefaultHttpContext context = new DefaultHttpContext();
            OperationCanceledException exception = new OperationCanceledException();

            ProblemDetails problemDetails = new ProblemDetails()
            {
               Status = StatusCodes.Status499ClientClosedRequest
            };

            mockProblemDetailsFactory.Setup(x => x.CreateProblemDetails(It.IsAny<HttpContext>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(problemDetails);

            ProblemDetailsContext? captured = null;

            mockProblemDetailsWriter
                .Setup(x => x.WriteAsync(It.IsAny<ProblemDetailsContext>()))
                .Callback<ProblemDetailsContext>(ctx => captured = ctx)
                .Returns(ValueTask.CompletedTask);

            // Act
            await _handler.TryHandleAsync(context, exception, CancellationToken.None);

            // Assert
            Assert.IsNotNull(captured);
            Assert.AreEqual(context, captured.HttpContext);
            Assert.AreEqual(problemDetails.Status, captured.ProblemDetails.Status);
        }
    }
}