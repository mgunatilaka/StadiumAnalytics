using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using StadiumAnalytics.Domain.Models;
using Polly;
using Polly.Retry;

namespace EventSimulator
{
    public class EventSimulatorWorker
    {
        private readonly HttpClient _client;
        private readonly ILogger<EventSimulatorWorker> _logger;
        private readonly Options _options;
        private readonly ConcurrentQueue<SensorEvent> _eventQueue = new ConcurrentQueue<SensorEvent>();
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

        public class Options
        {
            public string ApiBaseUrl { get; set; } = string.Empty;
            public string GateName { get; set; } = string.Empty;
        }

        public EventSimulatorWorker(HttpClient client, ILogger<EventSimulatorWorker> logger, Options options)
        {
            _client = client;
            _logger = logger;
            _options = options;

            _retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<HttpRequestException>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), 
                (result, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning("Request failed. Waiting {TimeSpan} before next retry. Retry attempt {RetryCount}", timeSpan, retryCount);
                });
        }

        public async Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("EventSimulatorWorker for {Gate} is starting.", _options.GateName);
            var random = new Random();

            _logger.LogInformation("Random event simulation for {Gate} started. Press 'q' and Enter to stop.", _options.GateName);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadLine();
                    if (key?.Trim().ToLower() == "q")
                    {
                        _logger.LogInformation("Random event simulation for {Gate} stopped by user.", _options.GateName);
                        break;
                    }
                }

                var evtEnter = new SensorEvent
                {
                    Id = Guid.NewGuid(),
                    Gate = _options.GateName,
                    Timestamp = DateTimeOffset.UtcNow,
                    NumberOfPeople = random.Next(1, 101),
                    Type = "enter"
                };

                var evtLeave = new SensorEvent
                {
                    Id = Guid.NewGuid(),
                    Gate = _options.GateName,
                    Timestamp = DateTimeOffset.UtcNow,
                    NumberOfPeople = random.Next(1, 101),
                    Type = "leave"
                };

                // Enqueue events locally
                _eventQueue.Enqueue(evtEnter);
                _eventQueue.Enqueue(evtLeave);

                // Try to process the queue if API is healthy
                var healthUrl = $"{_options.ApiBaseUrl.TrimEnd('/')}/health";
                HttpResponseMessage? healthResponse = null;
                try
                {
                    healthResponse = await _retryPolicy.ExecuteAsync(() => _client.GetAsync(healthUrl, stoppingToken));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("API health check failed for {Gate}: {Message}", _options.GateName, ex.Message);
                }

                if (healthResponse != null && healthResponse.IsSuccessStatusCode)
                {
                    await ProcessQueueAsync(stoppingToken);
                }
                else
                {
                    _logger.LogWarning("API is not healthy for {Gate}. Events will be kept in the local queue.", _options.GateName);
                }

                await Task.Delay(60000, stoppingToken);
            }
        }

        private async Task ProcessQueueAsync(CancellationToken stoppingToken)
        {
            while (_eventQueue.TryPeek(out var evt))
            {
                try
                {
                    var success = await PostSensorEventAsync(evt, stoppingToken);
                    if (success)
                    {
                        _eventQueue.TryDequeue(out _);
                    }
                    else
                    {
                        // Stop processing if failed, try again later
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Exception while processing event queue for {Gate}: {Message}", _options.GateName, ex.Message);
                    break;
                }
            }
        }

        private async Task<bool> PostSensorEventAsync(SensorEvent evt, CancellationToken stoppingToken)
        {
            var url = $"{_options.ApiBaseUrl.TrimEnd('/')}/api/v1/sensor-events";

            try
            {
                var response = await _retryPolicy.ExecuteAsync(async () =>
                {
                    using var request = new HttpRequestMessage(HttpMethod.Post, url)
                    {
                        Content = new StringContent(JsonSerializer.Serialize(evt), Encoding.UTF8, "application/json")
                    };
                    return await _client.SendAsync(request, stoppingToken);
                });

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Published event:{Timestamp} {Gate} {Type} {Count}", evt.Timestamp, evt.Gate, evt.Type, evt.NumberOfPeople);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to publish event for {Gate}. Status: {StatusCode}", evt.Gate, response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Critical failure publishing event for {Gate}: {Message}", evt.Gate, ex.Message);
                return false;
            }
        }
    }
}
