using StadiumAnalytics.Domain.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StadiumAnalytics.Domain.Interface.Messaging
{
    public interface ISensorEventChannel
    {
        Task PublishAsync(SensorEvent sensorEvent, CancellationToken cancellationToken = default);
        IAsyncEnumerable<SensorEvent> ReadAllAsync(CancellationToken cancellationToken = default);
    }
}