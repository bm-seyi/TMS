using Aspire.Hosting;
 
namespace TMS.IntegrationTests.API.Endpoints
{
    [TestClass]
    public sealed class HealthCheckTests
    {
        private readonly DistributedApplication app = GlobalTestSetup.App;
        public TestContext TestContext { get; set; }

       
        [TestMethod]
        public async Task HealthEndpoint_ShouldReturnSuccessStatusCode()
        {
            // Arrange
            HttpClient httpClient = app.CreateHttpClient("TMS-API", "https");
            await app.ResourceNotifications.WaitForResourceHealthyAsync("TMS-API", TestContext.CancellationToken);
 
            // Act
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("/health", TestContext.CancellationToken);
 
            // Assert
            Assert.IsTrue(httpResponseMessage.IsSuccessStatusCode);
        }
 
        [TestMethod]
        public async Task HealthEndpoint_ShouldReportDatabaseIsOnline()
        {
            // Arrange
            HttpClient httpClient = app.CreateHttpClient("TMS-API", "https");
            await app.ResourceNotifications.WaitForResourceHealthyAsync("TMS-API", TestContext.CancellationToken);

            HttpResponseMessage response = await httpClient.GetAsync("/health", TestContext.CancellationToken);
            response.EnsureSuccessStatusCode();
 
            // Act
            string content = await response.Content.ReadAsStringAsync(TestContext.CancellationToken);
 
 
            // Assert
            Assert.Contains("DatabaseHealthCheck", content);
            Assert.Contains("Database is online and operational", content);
 
        }
 
        [TestMethod]
        public async Task HealthEndpoint_ShouldReportContainerResourceUsage()
        {
            // Arrange
            HttpClient httpClient = app.CreateHttpClient("TMS-API", "https");
            await app.ResourceNotifications.WaitForResourceHealthyAsync("TMS-API", TestContext.CancellationToken);

            HttpResponseMessage response = await httpClient.GetAsync("/health", TestContext.CancellationToken);
            response.EnsureSuccessStatusCode();
 
            // Act
            string content = await response.Content.ReadAsStringAsync(TestContext.CancellationToken);
 
            // Assert
            Assert.Contains("container resources", content);
            Assert.Contains("CpuUsedPercentage", content);
            Assert.Contains("MemoryUsedPercentage", content);
        }
    }
}