using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Moq;
using TMS.Infrastructure.Factories;


namespace TMS.UnitTests.Persistence.Factories
{
    [TestClass]
    public sealed class SqlDatabaseFactoryTests
    {
        private Mock<ILogger<SqlConnectionFactory>> mockLogger = null!;
        private Mock<IConfiguration> mockConfiguration = null!;

        [TestInitialize]
        public void Setup()
        {
            mockLogger = new Mock<ILogger<SqlConnectionFactory>>();
            mockConfiguration = new Mock<IConfiguration>();
        }

        [TestMethod]
        public void ConstructorShouldThrowArgumentNullExceptionWhenDependenciesAreNull()
        {
            // Assert
            Assert.Throws<ArgumentNullException>(() => new SqlConnectionFactory(null!, mockLogger.Object));
            Assert.Throws<ArgumentNullException>(() => new SqlConnectionFactory(mockConfiguration.Object, null!));
        }
    }
}