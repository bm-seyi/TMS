using Microsoft.Extensions.Configuration;

namespace TMS.Core.Extensions
{
    public static class ConfigurationExtension
    {
        extension(IConfiguration configuration)
        {
            public T GetRequiredValue<T>(string key) => configuration.GetValue<T>(key) ?? throw new InvalidOperationException($"Configuration value for key '{key}' is required but not found");
        }
    }
}