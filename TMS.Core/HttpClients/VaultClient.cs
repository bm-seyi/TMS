using System.Diagnostics;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using TMS.Core.Interfaces.HttpClients;

namespace TMS.Core.HttpClients
{
    internal sealed class VaultClient : IVaultClient
    {
        private readonly ILogger<VaultClient> _logger;
        private readonly HttpClient _httpClient;
        private static readonly ActivitySource _activitySource = new ActivitySource("TMS.Core.HttpClients.VaultClient");

        public VaultClient(ILogger<VaultClient> logger, HttpClient httpClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<TVaultResponse> GetSecretAsync<TVaultResponse>(string path, CancellationToken cancellationToken = default)
        {
            using Activity? activity = _activitySource.StartActivity("VaultClient.GetSecretAsync");

            _logger.LogDebug("Starting secret retrieval from Vault. Path: {Path}", path);

            TVaultResponse? data = await _httpClient.GetFromJsonAsync<TVaultResponse>($"v1/secret/data/{path}", cancellationToken);
            
            if (data == null)
            {
                _logger.LogWarning("Vault returned null for path: {Path}", path);
                throw new InvalidOperationException($"Failed to retrieve secret from path: {path}");
            }
            
            _logger.LogInformation("Successfully retrieved secret from path: {Path}", path); 
        
            return data;
        }
    }
}