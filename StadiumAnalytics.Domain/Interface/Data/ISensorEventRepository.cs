using StadiumAnalytics.Domain.Models;
using System.Threading;
using System.Threading.Tasks;

namespace StadiumAnalytics.Infrastructure.Data
{
    public interface ISensorEventRepository
    {
        Task AddSensorEventAsync(SensorEvent sensorEvent, CancellationToken cancellationToken = default);
    }
}