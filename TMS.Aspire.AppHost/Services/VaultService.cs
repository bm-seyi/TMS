using System.Net.Http.Json;

namespace TMS.Aspire.AppHost.Services;

internal static class VaultService
{
    internal static async Task WriteArcGisPasswordAsync(ContainerResource resource, string password, CancellationToken ct)
    {
        EndpointReference endpoint = resource.GetEndpoint("http");

        using HttpClient httpClient = new HttpClient
        {
            BaseAddress = new Uri(endpoint.Url)
        };

        httpClient.DefaultRequestHeaders.Add("X-Vault-Token", "root");

        await WaitForVaultAsync(httpClient, ct);
        await WriteSecretAsync(httpClient, password, ct);
    }

    private static async Task WaitForVaultAsync(HttpClient httpClient, CancellationToken ct)
    {
        const int maxAttempts = 30;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                var response = await httpClient.GetAsync("v1/sys/health", ct);
                if (response.IsSuccessStatusCode)
                    return;
            }
            catch (HttpRequestException)
            {
            }

            await Task.Delay(1000, ct);
        }

        throw new TimeoutException("Vault did not become healthy in time.");
    }

    private static async Task WriteSecretAsync(HttpClient httpClient, string password, CancellationToken ct)
    {
        var secretData = new
        {
            data = new
            {
                Password = password
            }
        };

        HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            "v1/secret/data/arcgis",
            secretData,
            ct);

        response.EnsureSuccessStatusCode();
    }
}
