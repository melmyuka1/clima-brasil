using Microsoft.EntityFrameworkCore;
using WeatherDashboard.Domain.Entities;

namespace WeatherDashboard.Infrastructure.Data;

public class WeatherDbContext : DbContext
{
    public WeatherDbContext(DbContextOptions<WeatherDbContext> options) : base(options)
    {
    }

    public DbSet<WeatherRecord> WeatherRecords => Set<WeatherRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WeatherRecord>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.CityId).IsRequired().HasMaxLength(64);
            entity.Property(r => r.CityName).IsRequired().HasMaxLength(128);
            entity.Property(r => r.Uf).IsRequired().HasMaxLength(2);
            entity.Property(r => r.WeatherMain).IsRequired().HasMaxLength(64);
            entity.Property(r => r.WeatherDescription).IsRequired().HasMaxLength(128);
            entity.Property(r => r.WeatherIcon).IsRequired().HasMaxLength(16);
            entity.HasIndex(r => new { r.CityId, r.ObservedAtUtc });
        });
    }
}
