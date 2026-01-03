using System.Text;

namespace TMS.Aspire.AppHost
{
    public static class DebeziumService
    {
        public static async Task AddAsync(CancellationToken cancellationToken = default)
        {
            int counter = 0;
            while (counter < 20)
            {
                counter++;
                try
                {
                    using HttpClient httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:8083") };
                    string payload = await File.ReadAllTextAsync("./resources/kafka/sqlserver-connector.json", cancellationToken);
                    HttpResponseMessage response = await httpClient.PostAsync("/connectors", new StringContent(payload, Encoding.UTF8, "application/json"), cancellationToken);

                    if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        return;
                    }
                }
                catch
                {
                    continue;
                }

                await Task.Delay(10000, cancellationToken);
            }
            
        }
    }
}