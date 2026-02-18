using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using TMS.Core.Interfaces.Services;
using TMS.Core.Queries;
using TMS.Domain.Secrets;

namespace TMS.Core.Handlers
{
    public sealed class GetArcgisSecretHandler : IRequestHandler<GetArcgisSecretQuery, ArcgisSecret>
    {
        private readonly ILogger<GetArcgisSecretHandler> _logger;
        private readonly ISecretService _secretService;
        private static readonly ActivitySource _activitySource = new ActivitySource("TMS.Core.Handlers.GetArcgisSecretHandler");

        public GetArcgisSecretHandler(ILogger<GetArcgisSecretHandler> logger, ISecretService secretService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _secretService = secretService ?? throw new ArgumentNullException(nameof(secretService));
        }

        public async  Task<ArcgisSecret> Handle(GetArcgisSecretQuery request, CancellationToken cancellationToken)
        {
            using Activity? activity = _activitySource.StartActivity("GetArcgisSecretHandler.Handle");
            _logger.LogDebug("Handling GetArcgisSecretQuery.");

            ArcgisSecret arcgisSecret = await _secretService.GetSecretAsync<ArcgisVaultResponse, ArcgisSecret>("arcgis", cancellationToken);
            
            _logger.LogInformation("Successfully retrieved ArcGIS secret.");
            _logger.LogDebug("Completed handling of GetArcgisSecretQuery.");
            
            return arcgisSecret;
        }
    }
}