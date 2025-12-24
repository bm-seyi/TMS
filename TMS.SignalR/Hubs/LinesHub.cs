using System.Diagnostics;
using Microsoft.AspNetCore.SignalR;


namespace TMS.SignalR.Hubs
{
    public sealed class LinesHub : Hub
    {
        private readonly ILogger<LinesHub> _logger;
        private readonly static ActivitySource _activitySource = new ActivitySource("TMS.SignalR.Hubs.LinesHub");

        public LinesHub(ILogger<LinesHub> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task OnConnectedAsync()
        {
            using Activity? activity = _activitySource.StartActivity("LinesHub.OnConnectedAsync");

            _logger.LogInformation("Client connected to LinesHub: {ConnectionId}", Context.ConnectionId);

            await Clients.Caller.SendAsync("ReceiveLines", new {}, Context.ConnectionAborted);
        }
    }
}