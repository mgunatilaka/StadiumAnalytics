using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StadiumAnalytics.Api.Data;
using StadiumAnalytics.Shared.Models;
using StadiumAnalytics.Api.Services;

namespace StadiumAnalytics.Tests;

public class AnalyticsServiceTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetSummaryAsync_ReturnsCorrectSummary()
    {
        // Arrange
        var dbContext = GetDbContext();
        dbContext.SensorEvents.AddRange(
            new SensorEvent { Id = Guid.NewGuid(), Gate = "Gate A", Type = "enter", NumberOfPeople = 10, Timestamp = DateTimeOffset.UtcNow },
            new SensorEvent { Id = Guid.NewGuid(), Gate = "Gate A", Type = "enter", NumberOfPeople = 5, Timestamp = DateTimeOffset.UtcNow },
            new SensorEvent { Id = Guid.NewGuid(), Gate = "Gate A", Type = "leave", NumberOfPeople = 2, Timestamp = DateTimeOffset.UtcNow },
            new SensorEvent { Id = Guid.NewGuid(), Gate = "Gate B", Type = "enter", NumberOfPeople = 20, Timestamp = DateTimeOffset.UtcNow }
        );
        await dbContext.SaveChangesAsync();

        var service = new AnalyticsService(dbContext);

        // Act
        var result = (await service.GetSummaryAsync()).ToList();

        // Assert
        result.Should().HaveCount(3);
        result.Single(r => r.Gate == "Gate A" && r.Type == "enter").NumberOfPeople.Should().Be(15);
        result.Single(r => r.Gate == "Gate A" && r.Type == "leave").NumberOfPeople.Should().Be(2);
        result.Single(r => r.Gate == "Gate B" && r.Type == "enter").NumberOfPeople.Should().Be(20);
    }

    [Fact]
    public async Task GetSummaryAsync_FilterByGate_ReturnsOnlyThatGate()
    {
        // Arrange
        var dbContext = GetDbContext();
        dbContext.SensorEvents.AddRange(
            new SensorEvent { Id = Guid.NewGuid(), Gate = "Gate A", Type = "enter", NumberOfPeople = 10, Timestamp = DateTimeOffset.UtcNow },
            new SensorEvent { Id = Guid.NewGuid(), Gate = "Gate B", Type = "enter", NumberOfPeople = 20, Timestamp = DateTimeOffset.UtcNow }
        );
        await dbContext.SaveChangesAsync();

        var service = new AnalyticsService(dbContext);

        // Act
        var result = (await service.GetSummaryAsync(gate: "Gate A")).ToList();

        // Assert
        result.Should().HaveCount(1);
        result.Single().Gate.Should().Be("Gate A");
        result.Single().NumberOfPeople.Should().Be(10);
    }

    [Fact]
    public async Task GetSummaryAsync_FilterByTimeRange_ReturnsCorrectEvents()
    {
        // Arrange
        var dbContext = GetDbContext();
        var now = DateTimeOffset.UtcNow;
        dbContext.SensorEvents.AddRange(
            new SensorEvent { Id = Guid.NewGuid(), Gate = "Gate A", Type = "enter", NumberOfPeople = 10, Timestamp = now.AddMinutes(-10) },
            new SensorEvent { Id = Guid.NewGuid(), Gate = "Gate A", Type = "enter", NumberOfPeople = 5, Timestamp = now.AddMinutes(-5) },
            new SensorEvent { Id = Guid.NewGuid(), Gate = "Gate A", Type = "enter", NumberOfPeople = 20, Timestamp = now.AddMinutes(5) }
        );
        await dbContext.SaveChangesAsync();

        var service = new AnalyticsService(dbContext);

        // Act
        var startTime = now.AddMinutes(-6);
        var endTime = now;
        var result = (await service.GetSummaryAsync(startTime: startTime, endTime: endTime)).ToList();

        // Assert
        result.Should().HaveCount(1);
        result.Single().NumberOfPeople.Should().Be(5);
    }
}
