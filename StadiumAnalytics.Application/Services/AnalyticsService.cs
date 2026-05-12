using LinqKit;
using Microsoft.EntityFrameworkCore;
using StadiumAnalytics.Domain.Models;
using StadiumAnalytics.Infrastructure.Data;
using StadiumAnalytics.Application.DTOs;

namespace StadiumAnalytics.Application.Services;

public class AnalyticsService(AppDbContext dbContext) : IAnalyticsService
{
    

    public async Task<IEnumerable<AnalyticsSummaryDto>> GetSummaryAsync(
        string? gate = null,
        string? type = null,
        DateTimeOffset? startTime = null,
        DateTimeOffset? endTime = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.SensorEvents.AsQueryable();

        query = BuildFilteredQuery(gate, type, startTime, endTime, query);

        //logger.LogInformation("{gate}, {type}, {startTime}, {endTime}", gate, type, startTime!.Value, endTime!.Value);
        var groupedQuery = query
            .GroupBy(e => new { e.Gate, e.Type })
            .Select(g => new AnalyticsSummaryDto
            {
                Gate = g.Key.Gate,
                Type = g.Key.Type,
                NumberOfPeople = g.Sum(e => e.NumberOfPeople)
            });

        return await groupedQuery.ToListAsync(cancellationToken);
    }

    private static IQueryable<SensorEvent> BuildFilteredQuery(
        string? gate, string? type, DateTimeOffset? startTime, DateTimeOffset? endTime, IQueryable<SensorEvent> query)
    {
        var predicate = PredicateBuilder.New<SensorEvent>(true); // true indicates the start of a new predicate with no conditions.

        if (!string.IsNullOrWhiteSpace(gate))
        {
            predicate = predicate.And(e => e.Gate == gate);
        }
        if (!string.IsNullOrWhiteSpace(type))
        {
            predicate = predicate.And(e => e.Type == type);
        }
        if (startTime.HasValue)
        {
            predicate = predicate.And(e => e.Timestamp >= startTime.Value);
        }
        if (endTime.HasValue)
        {
            predicate = predicate.And(e => e.Timestamp <= endTime.Value);
        }

        return query.AsExpandable().Where(predicate);
    }
}
