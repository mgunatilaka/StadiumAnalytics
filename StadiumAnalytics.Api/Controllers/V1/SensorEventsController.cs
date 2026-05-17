using Microsoft.AspNetCore.Mvc;
using StadiumAnalytics.Domain.Models;
using StadiumAnalytics.Domain.Interface.Messaging;
using StadiumAnalytics.Application.DTOs;
using FluentValidation;

namespace StadiumAnalytics.Api.Controllers.V1;

[ApiController]
[Route("api/v1/sensor-events")]
public class SensorEventsController(
    ISensorEventChannel channel, 
    ILogger<SensorEventsController> logger,
    IValidator<SensorEventCreateDto> validator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] SensorEventCreateDto sensorEventDto, CancellationToken cancellationToken)
    {
        if (sensorEventDto == null)
        {
            return BadRequest("Invalid payload.");
        }

        var validationResult = await validator.ValidateAsync(sensorEventDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        var sensorEvent = new SensorEvent
        {
            Id = sensorEventDto.Id == Guid.Empty ? Guid.NewGuid() : sensorEventDto.Id,
            Gate = sensorEventDto.Gate,
            Timestamp = sensorEventDto.Timestamp,
            NumberOfPeople = sensorEventDto.NumberOfPeople,
            Type = sensorEventDto.Type
        };

        await channel.PublishAsync(sensorEvent, cancellationToken);
        logger.LogInformation("Received and published event for Gate: {Gate}", sensorEvent.Gate);
        return Accepted();
    }
}
