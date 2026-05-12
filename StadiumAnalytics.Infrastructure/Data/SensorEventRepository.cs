

using StadiumAnalytics.Domain.Models;

namespace StadiumAnalytics.Infrastructure.Data;

public class SensorEventRepository(AppDbContext dbContext) : ISensorEventRepository
{

    public async Task AddSensorEventAsync(SensorEvent sensorEvent, CancellationToken cancellationToken = default)
    {
        dbContext.SensorEvents.Add(sensorEvent);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

