using StadiumAnalytics.Api.DTOs;

namespace StadiumAnalytics.Api.Services;

public interface IAnalyticsService
{
    Task<IEnumerable<AnalyticsSummaryDto>> GetSummaryAsync(
        string? gate = null,
        string? type = null,
        DateTimeOffset? startTime = null,
        DateTimeOffset? endTime = null,
        CancellationToken cancellationToken = default);
}
