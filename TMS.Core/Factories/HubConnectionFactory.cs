using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TMS.Core.Interfaces.Factories;

namespace TMS.Core.Factories
{
    internal sealed class HubConnectionFactory : IHubConnectionFactory
    {
        private readonly ILogger<HubConnectionFactory> _logger;
        private readonly string _signalrUrl;

        public HubConnectionFactory(ILogger<HubConnectionFactory> logger, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _signalrUrl = configuration["SignalR:URL"] ?? throw new InvalidOperationException("SignalR URL is not configured. Please set 'SignalR:URL' in your appsettings.json or environment variables.");
        }

        public HubConnection CreateConnection(string relativeHubUrl)
        {
            HubConnection hubConnection = new HubConnectionBuilder()
            .WithUrl($"{_signalrUrl}{relativeHubUrl}")
            .WithAutomaticReconnect()
            .Build();

            _logger.LogInformation("Created SignalR connection to hub at {HubUrl}", $"{_signalrUrl}{relativeHubUrl}");

            return hubConnection;
        }
    }
}