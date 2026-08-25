using Microsoft.EntityFrameworkCore;
using WeatherDashboard.Domain.Entities;
using WeatherDashboard.Infrastructure.Data;
using Xunit;

namespace WeatherDashboard.Tests.Infrastructure;

public class EfWeatherRecordRepositoryTests
{
    private static WeatherDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<WeatherDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new WeatherDbContext(options);
    }

    private static WeatherRecord Record(string cityId, DateTime observedAtUtc) => new()
    {
        CityId = cityId,
        CityName = cityId,
        Uf = "XX",
        ObservedAtUtc = observedAtUtc,
        CollectedAtUtc = observedAtUtc,
        TemperatureC = 20,
        FeelsLikeC = 20,
        TempMinC = 18,
        TempMaxC = 22,
        HumidityPercent = 50,
        PressureHPa = 1013,
        WindSpeedMs = 3,
        WeatherMain = "Clear",
        WeatherDescription = "céu limpo",
        WeatherIcon = "01d",
    };

    [Fact]
    public async Task AddAsync_PersistsRecord()
    {
        await using var context = CreateContext();
        var repository = new EfWeatherRecordRepository(context);

        await repository.AddAsync(Record("sao-paulo", DateTime.UtcNow));

        Assert.Equal(1, await context.WeatherRecords.CountAsync());
    }

    [Fact]
    public async Task GetByCityAndRangeAsync_FiltersByCityAndDateRange()
    {
        await using var context = CreateContext();
        var repository = new EfWeatherRecordRepository(context);

        var today = new DateTime(2026, 8, 25, 10, 0, 0, DateTimeKind.Utc);
        await repository.AddAsync(Record("sao-paulo", today));
        await repository.AddAsync(Record("sao-paulo", today.AddDays(-10))); // fora do período
        await repository.AddAsync(Record("rio-de-janeiro", today)); // outra cidade

        var result = await repository.GetByCityAndRangeAsync(
            "sao-paulo", DateOnly.FromDateTime(today.AddDays(-1)), DateOnly.FromDateTime(today.AddDays(1)));

        var record = Assert.Single(result);
        Assert.Equal("sao-paulo", record.CityId);
    }

    [Fact]
    public async Task GetByCityAndRangeAsync_ReturnsOrderedByObservedAtUtc()
    {
        await using var context = CreateContext();
        var repository = new EfWeatherRecordRepository(context);

        var baseTime = new DateTime(2026, 8, 25, 8, 0, 0, DateTimeKind.Utc);
        await repository.AddAsync(Record("sao-paulo", baseTime.AddHours(6)));
        await repository.AddAsync(Record("sao-paulo", baseTime));
        await repository.AddAsync(Record("sao-paulo", baseTime.AddHours(3)));

        var result = await repository.GetByCityAndRangeAsync(
            "sao-paulo", DateOnly.FromDateTime(baseTime), DateOnly.FromDateTime(baseTime));

        Assert.Equal(3, result.Count);
        Assert.True(result[0].ObservedAtUtc < result[1].ObservedAtUtc);
        Assert.True(result[1].ObservedAtUtc < result[2].ObservedAtUtc);
    }
}
