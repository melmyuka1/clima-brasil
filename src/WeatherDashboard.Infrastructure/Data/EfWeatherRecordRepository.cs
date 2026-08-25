using Microsoft.EntityFrameworkCore;
using WeatherDashboard.Domain.Entities;
using WeatherDashboard.Domain.Interfaces;

namespace WeatherDashboard.Infrastructure.Data;

public class EfWeatherRecordRepository : IWeatherRecordRepository
{
    private readonly WeatherDbContext _context;

    public EfWeatherRecordRepository(WeatherDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(WeatherRecord record, CancellationToken cancellationToken = default)
    {
        _context.WeatherRecords.Add(record);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WeatherRecord>> GetByCityAndRangeAsync(
        string cityId, DateOnly start, DateOnly end, CancellationToken cancellationToken = default)
    {
        var startUtc = start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var endUtc = end.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        return await _context.WeatherRecords
            .Where(r => r.CityId == cityId && r.ObservedAtUtc >= startUtc && r.ObservedAtUtc <= endUtc)
            .OrderBy(r => r.ObservedAtUtc)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
