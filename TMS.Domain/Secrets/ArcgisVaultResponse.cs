namespace TMS.Domain.Secrets
{
    public sealed record ArcgisVaultResponse
    {
        public required string ApiKey { get; init; }
    }
}