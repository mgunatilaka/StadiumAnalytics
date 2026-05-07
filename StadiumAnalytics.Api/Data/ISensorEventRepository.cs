using StadiumAnalytics.Shared.Models;

namespace StadiumAnalytics.Api.Data
{
    public interface ISensorEventRepository
    {
        Task AddSensorEventAsync(SensorEvent sensorEvent, CancellationToken cancellationToken = default);
    }
}