using System;

namespace StadiumAnalytics.Application.DTOs;

public class SensorEventCreateDto
{
    public Guid Id { get; set; }
    public string Gate { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public int NumberOfPeople { get; set; }
    public string Type { get; set; } = string.Empty;
}
