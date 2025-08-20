
namespace TMS_API.Models.Configuration
{
    public class JwtOptions
    {
        public required string Authority { get; set; }
        public bool RequireHttpsMetadata { get; set; } = true;
        public required JwtTokenValidationParameters TokenValidationParameters { get; set; }
    }

    public class JwtTokenValidationParameters
    {
        public required string ValidIssuer { get; set; }
        public required string ValidAudience { get; set; }
        public bool ValidateIssuerSigningKey { get; set; } = true;
        public bool ValidateIssuer { get; set; } = true;
        public bool ValidateAudience { get; set; } = true;
        public bool ValidateLifetime { get; set; } = true;
        public int ClockSkew { get; set; } = 300;
    }
}