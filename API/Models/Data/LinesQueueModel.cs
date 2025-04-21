namespace TMS_API.Models.Data
{
    public class LinesQueueModel
    {
        public Guid Id { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? ChangeType {get; set; }

    }

}