using StadiumAnalytics.Shared.DTOs;

namespace StadiumAnalytics.Application.Services;

public interface IAnalyticsService
{
    Task<IEnumerable<AnalyticsSummaryDto>> GetSummaryAsync(
        string? gate = null,
        string? type = null,
        DateTimeOffset? startTime = null,
        DateTimeOffset? endTime = null,
        CancellationToken cancellationToken = default);
}
