using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using StadiumAnalytics.Infrastructure.Data;
using StadiumAnalytics.Application.Workers;
using StadiumAnalytics.Infrastructure.Messaging;
using StadiumAnalytics.Shared.Models;
using Xunit;
namespace StadiumAnalytics.Tests;
public class EventConsumerWorkerTests
{

    private async IAsyncEnumerable<SensorEvent> GetEvents(SensorEvent evt)
    {
        yield return evt;
        await Task.CompletedTask;
    }

    [Fact]
    public async Task ExecuteAsync_ConsumesEventsAndCallsProcessSensorEventAsync()
    {
        // Arrange
        var sensorEvent = new SensorEvent { Gate = "GateA", Type = "enter", NumberOfPeople = 1 };
        var channelMock = new Mock<ISensorEventChannel>();
        channelMock
            .Setup(c => c.ReadAllAsync(It.IsAny<CancellationToken>()))
            .Returns(GetEvents(sensorEvent));

        var repoMock = new Mock<ISensorEventRepository>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var scopeMock = new Mock<IServiceScope>();
        var scopeFactoryMock = new Mock<IServiceScopeFactory>();

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
            .Returns(scopeFactoryMock.Object);

        scopeFactoryMock
            .Setup(sf => sf.CreateScope())
            .Returns(scopeMock.Object);

        scopeMock
            .Setup(s => s.ServiceProvider)
            .Returns(serviceProviderMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ISensorEventRepository)))
            .Returns(repoMock.Object);

        var loggerMock = new Mock<ILogger<EventConsumerWorker>>();

        var workerMock = new Mock<EventConsumerWorker>(
            channelMock.Object, serviceProviderMock.Object, loggerMock.Object
        ) { CallBase = true };

        // Act
        var cts = new CancellationTokenSource();
        cts.CancelAfter(100);
        await workerMock.Object.StartAsync(cts.Token);

        // Assert
        workerMock.Protected().Verify(
            "ProcessSensorEventAsync",
            Times.AtLeastOnce(),
            ItExpr.Is<SensorEvent>(e => e == sensorEvent),
            ItExpr.IsAny<CancellationToken>()
        );
    }
}