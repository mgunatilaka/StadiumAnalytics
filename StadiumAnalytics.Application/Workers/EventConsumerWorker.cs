using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StadiumAnalytics.Domain.Interface.Messaging;
using StadiumAnalytics.Domain.Models;
using StadiumAnalytics.Infrastructure.Data;
using Polly;
using Polly.Retry;
using System.Text.Json;

namespace StadiumAnalytics.Application.Workers;

public class EventConsumerWorker : BackgroundService
{
    private readonly ISensorEventChannel _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventConsumerWorker> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly string _dlqDirectory;

    public EventConsumerWorker(ISensorEventChannel channel, IServiceProvider serviceProvider, ILogger<EventConsumerWorker> logger)
    {
        _channel = channel;
        _serviceProvider = serviceProvider;
        _logger = logger;

        _dlqDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DeadLetterQueue");
        if (!Directory.Exists(_dlqDirectory))
        {
            Directory.CreateDirectory(_dlqDirectory);
        }

        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), 
            (exception, timeSpan, retryCount, context) =>
            {
                _logger.LogWarning(exception, "Database save failed. Waiting {TimeSpan} before next retry. Retry attempt {RetryCount}", timeSpan, retryCount);
            });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EventConsumerWorker is starting.");

        try
        {
            await foreach (var sensorEvent in _channel.ReadAllAsync(stoppingToken))
            {
                await ProcessSensorEventAsync(sensorEvent, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("EventConsumerWorker is stopping.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while consuming events.");
        }
    }

    // Extracted for unit testing
    protected virtual async Task ProcessSensorEventAsync(SensorEvent sensorEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Consuming event for Gate: {Gate}", sensorEvent.Gate);

        try
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<ISensorEventRepository>();
                await repository.AddSensorEventAsync(sensorEvent, cancellationToken);
            });
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to save event to database after multiple retries. Sending to Dead Letter Queue. Event ID: {Id}", sensorEvent.Id);
            await SendToDeadLetterQueueAsync(sensorEvent);
        }
    }

    private async Task SendToDeadLetterQueueAsync(SensorEvent sensorEvent)
    {
        try
        {
            var fileName = Path.Combine(_dlqDirectory, $"{sensorEvent.Id}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.json");
            var json = JsonSerializer.Serialize(sensorEvent);
            await File.WriteAllTextAsync(fileName, json);
            _logger.LogInformation("Event {Id} successfully written to DLQ at {FileName}", sensorEvent.Id, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "FATAL: Failed to write event {Id} to Dead Letter Queue! Data may be lost.", sensorEvent.Id);
        }
    }
}

