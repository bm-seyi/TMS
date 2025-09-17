using System.Diagnostics.CodeAnalysis;

namespace Models.Dtos
{
    [ExcludeFromCodeCoverage]
    public class DatabaseHealthCheckDto
    {
        public required string DatabaseName { get; set; }
        public required string Status { get; set; }
    }
}