using System;
using System.Threading.Tasks;
using Serilog;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Microsoft.Extensions.Logging;

namespace EventSimulator;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Setup Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSerilog(dispose: true);
        });

        // Setup OpenTelemetry Tracing
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("EventSimulator"))
            .AddHttpClientInstrumentation()
            .AddConsoleExporter()
            .Build();

        // You can set the API base URL here or get from args/config
        var apiBaseUrl = args.Length > 0 ? args[0] : "http://localhost:5115";       
        var gates = new[] { "Gate A", "Gate B", "Gate C" };

        using var httpClient = new System.Net.Http.HttpClient();

        var cts = new System.Threading.CancellationTokenSource();
        Console.CancelKeyPress += (s, e) => {
            e.Cancel = true;
            cts.Cancel();
        };

        var tasks = new System.Collections.Generic.List<Task>();
        foreach (var gate in gates)
        {
            var worker = new EventSimulatorWorker(
                httpClient,
                loggerFactory.CreateLogger<EventSimulatorWorker>(),
                new EventSimulatorWorker.Options { ApiBaseUrl = apiBaseUrl, GateName = gate }
            );
            tasks.Add(worker.StartAsync(cts.Token));
        }

        Log.Information("Started all EventSimulatorWorker instances. Press Ctrl+C to exit.");
        await Task.WhenAll(tasks);
        
        Log.CloseAndFlush();
    }
}
