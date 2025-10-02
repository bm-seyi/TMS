using System.Text;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Aspire
{
    public sealed class DebeziumHealth : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            int counter = 0;
            while (counter < 20)
            {
                counter++;
                try
                {
                    using HttpClient httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:8083") };
                    string payload = await File.ReadAllTextAsync("./resources/kafka/sqlserver-connector.json");
                    HttpResponseMessage response = await httpClient.PostAsync("/connectors", new StringContent(payload, Encoding.UTF8, "application/json"));

                    if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        return HealthCheckResult.Healthy("Debezium is healthy");
                    }
                }
                catch
                {
                    continue;
                }

                await Task.Delay(10000, cancellationToken);
            }
            
            return HealthCheckResult.Unhealthy("Debezium is unhealthy");
        }
    }
}