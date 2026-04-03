using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Http;
using TMS.API.ExceptionHandlers;


namespace TMS.UnitTests.API.ExceptionHandlers
{
    [TestClass]
    public sealed class ExceptionHandlerTests
    {
        private Mock<ILogger<ExceptionHandler>> mockLogger = null!;
        private Mock<IProblemDetailsWriter> mockProblemDetailsWriter = null!;
        private Mock<ProblemDetailsFactory> mockProblemDetailsFactory = null!;

        private ExceptionHandler _handler = null!;

        [TestInitialize]
        public void Setup()
        {
            mockLogger = new Mock<ILogger<ExceptionHandler>>();
            mockProblemDetailsWriter = new Mock<IProblemDetailsWriter>();
            mockProblemDetailsFactory =  new Mock<ProblemDetailsFactory>();
            _handler = new ExceptionHandler(mockLogger.Object,mockProblemDetailsWriter.Object, mockProblemDetailsFactory.Object);
        }

        
        [TestMethod]
        public void ConstructorShouldThrowArgumentNullExceptionWhenDependenciesAreNull()
        {
            // Assert
            Assert.Throws<ArgumentNullException>(() => new ExceptionHandler(null!,mockProblemDetailsWriter.Object, mockProblemDetailsFactory.Object));
            Assert.Throws<ArgumentNullException>(() => new ExceptionHandler(mockLogger.Object,null!, mockProblemDetailsFactory.Object));
            Assert.Throws<ArgumentNullException>(() => new ExceptionHandler(mockLogger.Object,mockProblemDetailsWriter.Object, null!));
        }
        

        [TestMethod]
        public async Task TryHandleAsync_ShouldWriteProblemDetails_AndReturnTrue()
        {
            // Arrange
            DefaultHttpContext context = new DefaultHttpContext();
            context.Request.Path = "/test-path";

            Exception exception = new Exception("test exception");

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
        public async Task TryHandleAsync_ShouldLogError()
        {
            // Arrange
            DefaultHttpContext context = new DefaultHttpContext();
            context.Request.Path = "/log-test";

            Exception exception = new Exception("failure");

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
        public async Task TryHandleAsync_ShouldPassCorrectHttpContext_ToWriter()
        {
            // Arrange
            DefaultHttpContext context = new DefaultHttpContext();
            context.Request.Path = "/writer-context";

            Exception exception = new Exception();

            ProblemDetailsContext? capturedContext = null;

            mockProblemDetailsWriter
                .Setup(x => x.WriteAsync(It.IsAny<ProblemDetailsContext>()))
                .Callback<ProblemDetailsContext>(ctx => capturedContext = ctx)
                .Returns(ValueTask.CompletedTask);

            // Act
            await _handler.TryHandleAsync(context, exception, CancellationToken.None);

            // Assert
            Assert.IsNotNull(capturedContext);
            Assert.AreEqual(context, capturedContext.HttpContext);
        }
    }
}
 