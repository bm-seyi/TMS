using System.Diagnostics.CodeAnalysis;

namespace TMS.Models.DTOs
{
    [ExcludeFromCodeCoverage]
    public class DatabaseHealthCheckDTO
    {
        public required string DatabaseName { get; set; }
        public required string Status { get; set; }
    }
}