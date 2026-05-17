using Microsoft.EntityFrameworkCore;
using StadiumAnalytics.Domain.Models;

namespace StadiumAnalytics.Infrastructure.Data;

public class SensorEventRepository(AppDbContext dbContext) : ISensorEventRepository
{

    public async Task AddSensorEventAsync(SensorEvent sensorEvent, CancellationToken cancellationToken = default)
    {
        var exists = await dbContext.SensorEvents.AnyAsync(e => e.Id == sensorEvent.Id, cancellationToken);
        if (exists)
        {
            // Event already processed, skip to ensure idempotency
            return;
        }

        dbContext.SensorEvents.Add(sensorEvent);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

