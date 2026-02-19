using System.Diagnostics;
using MediatR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using TMS.Application.Events;
using TMS.Application.Interfaces.Factories;
using TMS.Application.Interfaces.Infrastructure.HubClients;

namespace TMS.Application.NotificationHandlers
{
    public sealed class LinesUpdatedHandler : INotificationHandler<LinesUpdatedNotification>
    {
        private readonly ILogger<LinesUpdatedHandler> _logger;
        private readonly ILinesDataHubClient _linesDataHubClient;
        private static readonly ActivitySource _activitySource = new ActivitySource("TMS.Application.NotificationHandlers.LinesUpdatedHandler");

        public LinesUpdatedHandler(ILogger<LinesUpdatedHandler> logger,  ILinesDataHubClient linesDataHubClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _linesDataHubClient = linesDataHubClient ?? throw new ArgumentNullException(nameof(linesDataHubClient));
        }

        public async Task Handle(LinesUpdatedNotification notification, CancellationToken cancellationToken)
        {
           using Activity? _ = _activitySource.StartActivity("LinesUpdatedHandler.Handle");

           await _linesDataHubClient.NotifyLinesUpdatedAsync(notification, cancellationToken);
        }
    }
}