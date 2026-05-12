using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using StadiumAnalytics.Api.Controllers;
using StadiumAnalytics.Domain.Interface.Messaging;
using StadiumAnalytics.Domain.Models;
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
        var controller = new SensorEventsController(channelMock.Object, loggerMock.Object);
        var sensorEvent = new SensorEvent { Gate = "GateA", Type = "enter", NumberOfPeople = 1 };

        // Act
        var result = await controller.Post(sensorEvent, CancellationToken.None);

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
        var controller = new SensorEventsController(channelMock.Object, loggerMock.Object);

        // Act
        var result = await controller.Post(null, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestResult>(result);
        channelMock.Verify(c => c.PublishAsync(It.IsAny<SensorEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}