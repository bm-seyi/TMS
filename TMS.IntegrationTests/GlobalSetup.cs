using System.Text.Json;
using Aspire.Hosting;
using Azure.Core;
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
            .AddUserSecrets<GlobalTestSetup>()
            .AddEnvironmentVariables()
            .Build();
 
            IDistributedApplicationTestingBuilder builder = await DistributedApplicationTestingBuilder
                .CreateAsync<TMS_Aspire_AppHost>();
 
            App = await builder.BuildAsync();
            await App.StartAsync();
        }
 
 
        public static async Task<string> GetAccessTokenAsync()
        {
            using HttpClient http = new HttpClient();
 
            string clientSecret = Configuration.GetValue<string>("IDP:ClientSecret") ?? throw new InvalidOperationException("Unable to retrieve the client secret");
 
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://idp-tst-web.diversitytravel.com/connect/token");
            Dictionary<string, string> formData = new Dictionary<string, string>
            {
                ["client_id"] = "Postman_Client",
                ["client_secret"] = clientSecret,
                ["grant_type"] = "client_credentials",
            };
 
            request.Content = new FormUrlEncodedContent(formData);
 
            HttpResponseMessage response = await http.SendAsync(request);
            response.EnsureSuccessStatusCode();
 
            JsonDocument payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return payload.RootElement.GetProperty("access_token").GetString()!;
        }
 
        [AssemblyCleanup]
        public static async Task AssemblyCleanup()
        {
            if (App != null)
            {
                await App.StopAsync();
                await App.DisposeAsync();
            }
        }
    }
}