using StadiumAnalytics.Api.Data;
using StadiumAnalytics.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using StadiumAnalytics.Shared.Models;

namespace StadiumAnalytics.Api.Data;

public class SensorEventRepository(AppDbContext dbContext) : ISensorEventRepository
{

    public async Task AddSensorEventAsync(SensorEvent sensorEvent, CancellationToken cancellationToken = default)
    {
        dbContext.SensorEvents.Add(sensorEvent);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

