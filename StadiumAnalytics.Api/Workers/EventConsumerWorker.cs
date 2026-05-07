using StadiumAnalytics.Api.Data;
using StadiumAnalytics.Shared.Messaging;

namespace StadiumAnalytics.Api.Workers;

public class EventConsumerWorker : BackgroundService
{
    private readonly ISensorEventChannel _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventConsumerWorker> _logger;

    public EventConsumerWorker(ISensorEventChannel channel, IServiceProvider serviceProvider, ILogger<EventConsumerWorker> logger)
    {
        _channel = channel;
        _serviceProvider = serviceProvider;
        _logger = logger;
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
    protected virtual async Task ProcessSensorEventAsync(Shared.Models.SensorEvent sensorEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Consuming event for Gate: {Gate}", sensorEvent.Gate);

        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISensorEventRepository>();

        await repository.AddSensorEventAsync(sensorEvent, cancellationToken);
    }
}

