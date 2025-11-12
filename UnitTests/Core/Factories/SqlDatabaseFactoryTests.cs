using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Moq;
using Core.Factories;

namespace UnitTests.Core.Factories
{
    [TestClass]
    public sealed class SqlDatabaseFactoryTests
    {
        private Mock<ILogger<SqlDatabaseFactory>> mockLogger = null!;
        private Mock<IConfiguration> mockConfiguration = null!;

        [TestInitialize]
        public void Setup()
        {
            mockLogger = new Mock<ILogger<SqlDatabaseFactory>>();
            mockConfiguration = new Mock<IConfiguration>();
        }

        [TestMethod]
        public void ConstructorShouldThrowArgumentNullExceptionWhenDependenciesAreNull()
        {
            // Assert
            Assert.Throws<ArgumentNullException>(() => new SqlDatabaseFactory(null!, mockLogger.Object));
            Assert.Throws<ArgumentNullException>(() => new SqlDatabaseFactory(mockConfiguration.Object, null!));
        }
    }
}