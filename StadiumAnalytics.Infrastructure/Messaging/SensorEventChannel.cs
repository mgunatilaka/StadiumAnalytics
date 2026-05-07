using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using StadiumAnalytics.Shared.Models;

namespace StadiumAnalytics.Infrastructure.Messaging
{
    public class SensorEventChannel : ISensorEventChannel
    {
        private readonly Channel<SensorEvent> _channel;

        public SensorEventChannel()
        {
            var options = new BoundedChannelOptions(10_000)
            {
                SingleWriter = false,
                SingleReader = true
            };
            _channel = Channel.CreateBounded<SensorEvent>(options);
        }

        public async Task PublishAsync(SensorEvent sensorEvent, CancellationToken cancellationToken = default)
        {
            await _channel.Writer.WriteAsync(sensorEvent, cancellationToken);
        }

        public IAsyncEnumerable<SensorEvent> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            return _channel.Reader.ReadAllAsync(cancellationToken);
        }
    }
}
