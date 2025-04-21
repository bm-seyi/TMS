using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TMS_API.Models.Data;
using TMS_API.Utilities;

namespace TMS_API.Hubs
{
    [Authorize]
    public class LinesHub : Hub
    {
        private readonly ILogger<LinesHub> _logger;
        private readonly IDatabaseActions _databaseActions;
        public LinesHub(ILogger<LinesHub> logger, IDatabaseActions databaseActions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseActions = databaseActions ?? throw new ArgumentNullException(nameof(databaseActions));
        }

        public override async Task OnConnectedAsync()
        {
            CancellationToken cancellationToken = Context.ConnectionAborted;
            _logger.LogInformation("Client connected to LinesHub: {0}", Context.ConnectionId);

            List<LinesModel> linesData = await _databaseActions.RetrieveModelAsync<LinesModel>("SELECT [Id], [Latitude], [Longitude] FROM [dbo].[Lines]", false, cancellationToken);

            await Clients.Caller.SendAsync("ReceiveLinesData", linesData, cancellationToken);
            _logger.LogInformation("Lines data sent to client: {0}", Context.ConnectionId);

            await base.OnConnectedAsync();
        }
    }
}