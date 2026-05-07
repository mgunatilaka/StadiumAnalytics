using StadiumAnalytics.Shared.Models;
using Microsoft.EntityFrameworkCore;
namespace StadiumAnalytics.Infrastructure.Data;
public class SensorEventRepository(AppDbContext dbContext) : ISensorEventRepository
{
    public async Task AddSensorEventAsync(SensorEvent sensorEvent, CancellationToken cancellationToken = default)
    {
        dbContext.SensorEvents.Add(sensorEvent);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
