using System.Diagnostics;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TMS.Application.Extensions;
using TMS.Application.Interfaces.Factories;

namespace TMS.Application.Factories
{
    internal sealed class HubConnectionFactory : IHubConnectionFactory
    {
        private readonly ILogger<HubConnectionFactory> _logger;
        private readonly IConfiguration _configuration;
        private static readonly ActivitySource _activitySource = new ActivitySource("TMS.Core.Factories.HubConnectionFactory");

        public HubConnectionFactory(ILogger<HubConnectionFactory> logger, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public HubConnection CreateConnection(string relativeHubUrl)
        {
            using Activity? activity = _activitySource.StartActivity("HubConnectionFactory.CreateConnection");

            string signalrUrl = _configuration.GetRequiredValue<string>("SignalR:BaseUrl");

            HubConnection hubConnection = new HubConnectionBuilder()
            .WithUrl($"{signalrUrl}/{relativeHubUrl}")
            .WithAutomaticReconnect()
            .Build();

            _logger.LogInformation("Created SignalR connection to hub at {HubUrl}", $"{signalrUrl}{relativeHubUrl}");

            return hubConnection;
        }
    }
}