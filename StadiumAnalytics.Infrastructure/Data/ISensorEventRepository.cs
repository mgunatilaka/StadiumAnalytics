using StadiumAnalytics.Shared.Models;
namespace StadiumAnalytics.Infrastructure.Data;
public interface ISensorEventRepository
{
    Task AddSensorEventAsync(SensorEvent sensorEvent, CancellationToken cancellationToken = default);
}
