using Microsoft.EntityFrameworkCore;
using StadiumAnalytics.Shared.Models;

namespace StadiumAnalytics.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<SensorEvent> SensorEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //configure the SensorEvent entity with DatetimeOffset conversion for SQLite compatibility
        modelBuilder.Entity<SensorEvent>()
             .Property(e => e.Timestamp)
             .HasConversion(
                 v => v.ToString(),
                 v => DateTimeOffset.Parse(v)
             );
        // Apply indices for faster querying
        modelBuilder.Entity<SensorEvent>()
             .HasIndex(e => new { e.Gate, e.Type, e.Timestamp });
    }
}
