
namespace TMS_API.Models.Configuration
{
    public class RedisOptions
    {
        public required string Endpoint { get; set; }
        public required string Password { get; set; }
        public bool UseSsl { get; set; }
        public int? Database { get; set; }
        public string ClientName { get; set; } = $"{Environment.MachineName}_{Environment.UserName}";
        public string ChannelPrefix { get; set; } = "TMS_API:SignalR";
        public int ConnectTimeout { get; set; } = 5000;
        public int SyncTimeout { get; set; } = 5000;
        public bool CheckCertificateRevocation { get; set; } = false;
        public int AsyncTimeout { get; set; } = 5000;
        public int KeepAlive { get; set; } = 60;
        public int ConnectRetry { get; set; } = 5;
        public bool AbortOnConnectFail { get; set; } = false;
        public bool ResolveDns { get; set; } = true;
        public bool IncludeDetailInExceptions { get; set; } = false;
        public bool IncludePerformanceCountersInExceptions { get; set; } = false;
        public bool AllowAdmin { get; set; } = false;
    }
}

