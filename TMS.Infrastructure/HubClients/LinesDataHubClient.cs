using System.Diagnostics;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TMS.Application.Interfaces.Infrastructure.HubClients;


namespace TMS.Infrastructure.HubClients
{
    internal sealed class LinesDataHubClient : HubClientBase, ILinesDataHubClient
    {
        private readonly ILogger<LinesDataHubClient> _logger;
        private static readonly ActivitySource _activitySource = new ActivitySource("TMS.Infrastructure.HubClients");

        public LinesDataHubClient(ILogger<LinesDataHubClient> logger, [FromKeyedServices("LinesDataHub")] HubConnection hubConnection) : base(hubConnection)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task NotifyLinesUpdatedAsync(object data, CancellationToken cancellationToken)
        {
            using Activity? _ = _activitySource.StartActivity("LinesDataHubClient.NotifyLinesUpdatedAsync");

            await _hubConnection.SendAsync("LinesUpdated", data, cancellationToken);
        }
    }
}