using System.Diagnostics;
using AutoMapper;
using Microsoft.Extensions.Logging;
using TMS.Application.Interfaces.Infrastructure.Http;
using TMS.Application.Interfaces.Services;
using TMS.Domain.Secrets;

namespace TMS.Application.Services
{
    internal sealed class SecretsService : ISecretService
    {
        private readonly ILogger<SecretsService> _logger;
        private readonly IVaultClient _vaultClient;
        private readonly IMapper _mapper;
        private static readonly ActivitySource _activitySource = new ActivitySource("TMS.Application.Services.SecretsService");
        public SecretsService(ILogger<SecretsService> logger, IVaultClient vaultClient, IMapper mapper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _vaultClient = vaultClient ?? throw new AbandonedMutexException(nameof(vaultClient));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<TSecret> GetSecretAsync<TVaultResponse, TSecret>(string path, CancellationToken cancellationToken)
        {
            using Activity? activity = _activitySource.StartActivity("SecretsService.GetSecretAsync");
            _logger.LogDebug("Starting secret retrieval and mapping. Path: {Path}", path);

            VaultResponse<TVaultResponse> secret = await _vaultClient.GetSecretAsync<VaultResponse<TVaultResponse>>(path, cancellationToken);

            _logger.LogDebug("Successfully retrieved secret from Vault. Path: {Path}", path);

            TSecret mappedSecret = _mapper.Map<TSecret>(secret);

            _logger.LogInformation("Successfully mapped secret to target type {TargetType} for path: {Path}", typeof(TSecret).Name, path);
            _logger.LogDebug("Completed secret retrieval and mapping for path: {Path}", path);

            return mappedSecret;
        }
    }
}