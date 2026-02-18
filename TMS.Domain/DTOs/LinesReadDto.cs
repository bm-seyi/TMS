namespace TMS.Domain.DTOs
{
    public sealed record LinesReadDTO
    {
        public Guid Id { get; init; }
        public double Latitude { get; init; }
        public double Longitude { get; init; }
        public required string LineCode { get; init; }
    }
}