namespace Models.Dtos
{
    public sealed record LinesReadDto
    {
        public Guid Id { get; init; }
        public required string OPM_Id { get; init; }
        public double Latitude { get; init; }
        public double Longitude { get; init; }
        public required string LineCode { get; init; }
    }
}