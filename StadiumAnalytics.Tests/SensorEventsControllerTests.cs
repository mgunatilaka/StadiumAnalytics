using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using StadiumAnalytics.Api.Controllers.V1;
using StadiumAnalytics.Application.DTOs;
using StadiumAnalytics.Domain.Interface.Messaging;
using StadiumAnalytics.Domain.Models;
using System.ComponentModel.DataAnnotations;
namespace StadiumAnalytics.Tests;
public class SensorEventsControllerTests
{
    [Fact]
    public async Task Post_ValidEvent_ReturnsAccepted()
    {
        // Arrange
        var channelMock = new Mock<ISensorEventChannel>();
        channelMock.Setup(c => c.PublishAsync(It.IsAny<SensorEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var loggerMock = new Mock<ILogger<SensorEventsController>>();
        var validationMock = new Mock<IValidator<SensorEventCreateDto>>() ;

        var controller = new SensorEventsController(channelMock.Object, loggerMock.Object,validationMock.Object);
        var sensorEventCreateDto = new SensorEventCreateDto { Gate = "GateA", Type = "enter", NumberOfPeople = 1, Id=Guid.NewGuid(), Timestamp= DateTimeOffset.UtcNow };
        var sensorEvent = new SensorEvent() { Gate = sensorEventCreateDto.Gate, Timestamp = sensorEventCreateDto.Timestamp, Id = sensorEventCreateDto.Id, NumberOfPeople = sensorEventCreateDto.NumberOfPeople, Type = sensorEventCreateDto.Type };
        validationMock.Setup(v => v.ValidateAsync(sensorEventCreateDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult() { RuleSetsExecuted = [], Errors = [] });
        // Act
        var result = await controller.Post(sensorEventCreateDto, CancellationToken.None);


        // Assert

        var acceptedResult = Assert.IsType<AcceptedResult>(result);
        channelMock.Verify(c => c.PublishAsync(sensorEvent, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Post_NullEvent_ReturnsBadRequest()
    {
        // Arrange
        var channelMock = new Mock<ISensorEventChannel>();
        var loggerMock = new Mock<ILogger<SensorEventsController>>();
        var validationMock = new Mock<IValidator<SensorEventCreateDto>>();
        var controller = new SensorEventsController(channelMock.Object, loggerMock.Object, validationMock.Object);

        // Act
        var result = await controller.Post(null, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        channelMock.Verify(c => c.PublishAsync(It.IsAny<SensorEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}