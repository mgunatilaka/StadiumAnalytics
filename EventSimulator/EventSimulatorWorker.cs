using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StadiumAnalytics.Shared.Messaging;
using StadiumAnalytics.Shared.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace EventSimulator
{
    public class EventSimulatorWorker
    {
        private readonly HttpClient _client;
        private readonly ILogger<EventSimulatorWorker> _logger;
        private readonly Options _options;

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
        }

        public async Task StartAsync(CancellationToken stoppingToken)
        {

            _logger.LogInformation("EventSimulatorWorker for {Gate} is starting.", _options.GateName);
            var random = new Random();
            var types = new[] { "enter", "leave" };

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

                await PostSensorEventAsync(evtEnter, stoppingToken);

                var evtLeave = new SensorEvent
                {
                    Id = Guid.NewGuid(),
                    Gate = _options.GateName,
                    Timestamp = DateTimeOffset.UtcNow,
                    NumberOfPeople = random.Next(1, 101),
                    Type = "leave"
                };

                await PostSensorEventAsync(evtLeave, stoppingToken);

                await Task.Delay(60000, stoppingToken);

            }
        }

         
        private async Task PostSensorEventAsync(SensorEvent evt, CancellationToken stoppingToken)
        {
            var url = $"{_options.ApiBaseUrl.TrimEnd('/')}/api/sensor-events";

            using var Request = new HttpRequestMessage();
            Request.Method = HttpMethod.Post;
            Request.RequestUri = new Uri(url);
            Request.Content = new StringContent(JsonSerializer.Serialize(evt), Encoding.UTF8, "application/json");

            using var response = await _client.SendAsync(Request);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Published event:{Timestamp} {Gate} {Type} {Count}", evt.Timestamp, evt.Gate, evt.Type, evt.NumberOfPeople);
            }
            else
            {
                _logger.LogWarning("Failed to publish event for {Gate}. Status: {StatusCode}", evt.Gate, response.StatusCode);
            }
        }
    }
}
