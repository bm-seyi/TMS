namespace TMS.Models.Secrets
{
    public sealed record ArcgisVaultResponse
    {
        public required string ApiKey { get; init; }
    }
}