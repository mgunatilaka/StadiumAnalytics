using Microsoft.EntityFrameworkCore;
using StadiumAnalytics.Api.Data;
using StadiumAnalytics.Shared.Models;

namespace StadiumAnalytics.Tests;
public class SensorEventRepositoryTests
{
    private AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "SensorEventTestDb")
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task AddSensorEventAsync_AddsEventToDatabase()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var repository = new SensorEventRepository(dbContext);
        var sensorEvent = new SensorEvent
        {
            Gate = "GateA",
            Type = "enter",
            NumberOfPeople = 1,
            Timestamp = DateTime.Now,
        };

        // Act
        await repository.AddSensorEventAsync(sensorEvent, CancellationToken.None);

        // Assert
        var savedEvent = await dbContext.SensorEvents.FirstOrDefaultAsync(e => e.Id == sensorEvent.Id);
        Assert.NotNull(savedEvent);
        var evt = (SensorEvent)savedEvent;
        Assert.Equal(sensorEvent.Gate, evt.Gate);
        Assert.Equal(sensorEvent.Type, evt.Type);
        Assert.Equal(sensorEvent.NumberOfPeople, evt.NumberOfPeople);
        Assert.Equal(sensorEvent.Timestamp, evt.Timestamp);
    }
}