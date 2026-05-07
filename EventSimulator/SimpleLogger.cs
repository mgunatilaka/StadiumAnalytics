using System;

namespace EventSimulator
{
    public partial class Program
    {
        // Simple logger implementation for demonstration
        private class SimpleLogger : Microsoft.Extensions.Logging.ILogger<EventSimulatorWorker>
        {
            private readonly string _gate;
            public SimpleLogger(string gate) => _gate = gate;
            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;
            public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                Console.WriteLine($"[{DateTimeOffset.Now:HH:mm:ss}] [{_gate}] {formatter(state, exception)}");
            }
        }
    }
}
