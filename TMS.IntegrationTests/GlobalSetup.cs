using Aspire.Hosting;
using Microsoft.Extensions.Configuration;
using Projects;
 
namespace TMS.IntegrationTests
{
    [TestClass]
    public class GlobalTestSetup
    {
        public static DistributedApplication App = null!;
        public static IConfiguration Configuration = null!;
 
        private GlobalTestSetup() { }

        [AssemblyInitialize]
        public static async Task AssemblyInit(TestContext context)
        {
            Configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();
 
            IDistributedApplicationTestingBuilder builder = await DistributedApplicationTestingBuilder
                .CreateAsync<TMS_Aspire_AppHost>();
 
            App = await builder.BuildAsync(context.CancellationToken);
            await App.StartAsync(context.CancellationToken);
        }
 
        [AssemblyCleanup]
        public static async Task AssemblyCleanup(TestContext context)
        {
            if (App != null)
            {
                await App.StopAsync(context.CancellationToken);
                await App.DisposeAsync();
            }
        }
    }
}