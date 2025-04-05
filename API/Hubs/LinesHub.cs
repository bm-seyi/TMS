using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TMS_API.Hubs
{
    [Authorize]
    public class LinesHub : Hub
    {
        private readonly ILogger<LinesHub> _logger;
        public LinesHub(ILogger<LinesHub> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
    }
}