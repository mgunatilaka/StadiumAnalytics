using StadiumAnalytics.Shared.Messaging;
using StadiumAnalytics.Shared.Models;

namespace StadiumAnalytics.Tests;
public class SensorEventChannelTests
{
    [Fact]
    public async Task PublishAsync_And_ReadAllAsync_WorkCorrectly()
    {
        // Arrange
        var channel = new SensorEventChannel();
        var events = new List<SensorEvent>
        {
            new SensorEvent { Gate = "GateA", Type = "enter", NumberOfPeople = 1, Timestamp = DateTime.UtcNow },
            new SensorEvent { Gate = "GateB", Type = "leave", NumberOfPeople = 2, Timestamp = DateTime.UtcNow.AddSeconds(1) }
        };

        // Act
        foreach (var sensorEvent in events)
        {
            await channel.PublishAsync(sensorEvent);
        }

        var readEvents = new List<SensorEvent>();
        await foreach (var evt in channel.ReadAllAsync())
        {
            readEvents.Add(evt);
            if (readEvents.Count == events.Count)
                break;
        }

        // Assert
        Assert.Equal(events.Count, readEvents.Count);
        for (int i = 0; i < events.Count; i++)
        {
            Assert.Equal(events[i].Gate, readEvents[i].Gate);
            Assert.Equal(events[i].NumberOfPeople, readEvents[i].NumberOfPeople);
            Assert.Equal(events[i].Timestamp, readEvents[i].Timestamp, TimeSpan.FromSeconds(1));
        }
    }
}