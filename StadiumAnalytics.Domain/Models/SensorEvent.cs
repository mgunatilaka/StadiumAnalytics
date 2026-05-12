using System;

namespace StadiumAnalytics.Domain.Models
{
    public class SensorEvent
    {
        public Guid Id { get; set; }
        public string Gate { get; set; } = string.Empty;
        public DateTimeOffset Timestamp { get; set; }
        public int NumberOfPeople { get; set; }
        public string Type { get; set; } = string.Empty;
    }
}
