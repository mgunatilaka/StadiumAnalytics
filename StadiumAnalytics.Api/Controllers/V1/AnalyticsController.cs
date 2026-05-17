using Microsoft.AspNetCore.Mvc;
using StadiumAnalytics.Application.DTOs;
using StadiumAnalytics.Application.Services;

namespace StadiumAnalytics.Api.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
public class AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger) : ControllerBase
{
    

    /// <summary>
    /// Gets a summary of people entering and leaving the stadium, grouped by gate and type.
    /// </summary>
    /// <param name="gate">Filter by gate name (e.g. 'Gate A')</param>
    /// <param name="type">Filter by event type (e.g. 'enter' or 'leave')</param>
    /// <param name="startTime">Optional start time filter</param>
    /// <param name="endTime">Optional end time filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of analytics summaries.</returns>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(IEnumerable<AnalyticsSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSummary(
        [FromQuery] string? gate,
        [FromQuery] string? type,
        [FromQuery] DateTimeOffset? startTime,
        [FromQuery] DateTimeOffset? endTime,
        CancellationToken cancellationToken)
    {
        try
        {
            var summary = await analyticsService.GetSummaryAsync(gate, type, startTime, endTime, cancellationToken);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while getting the analytics summary.");
            return Problem(
                detail: "An unexpected error occurred while processing your request.",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}
