using Microsoft.AspNetCore.Mvc;
using StadiumAnalytics.Shared.Models;
using StadiumAnalytics.Shared.Messaging;

namespace StadiumAnalytics.Api.Controllers;

[ApiController]
[Route("api/sensor-events")]
public class SensorEventsController(ISensorEventChannel channel, ILogger<SensorEventsController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] SensorEvent sensorEvent, CancellationToken cancellationToken)
    {
        if (sensorEvent == null)
        {
            return BadRequest();
        }
        await channel.PublishAsync(sensorEvent, cancellationToken);
        logger.LogInformation("Received and published event for Gate: {Gate}", sensorEvent.Gate);
        return Accepted();
    }
}
