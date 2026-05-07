using System;
using System.Threading.Tasks;

namespace EventSimulator;

public class Program
{
    public static async Task Main(string[] args)
    {
        // You can set the API base URL here or get from args/config
        var apiBaseUrl = args.Length > 0 ? args[0] : "http://localhost:5115";       
        var gates = new[] { "Gate A", "Gate B", "Gate C" };

        using var httpClient = new System.Net.Http.HttpClient();

        // Optionally, set up logging (simple console)  
        void Log(string message) => Console.WriteLine($"[{DateTimeOffset.Now:HH:mm:ss}] {message}");

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
                new SimpleLogger(gate),
                new EventSimulatorWorker.Options { ApiBaseUrl = apiBaseUrl, GateName = gate }
            );
            tasks.Add(worker.StartAsync(cts.Token));
        }

        Log("Started all EventSimulatorWorker instances. Press Ctrl+C to exit.");
        await Task.WhenAll(tasks);
    }
}
