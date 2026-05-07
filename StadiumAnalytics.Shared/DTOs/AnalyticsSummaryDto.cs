namespace StadiumAnalytics.Shared.DTOs;

public class AnalyticsSummaryDto
{
    public string Gate { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int NumberOfPeople { get; set; }
}
