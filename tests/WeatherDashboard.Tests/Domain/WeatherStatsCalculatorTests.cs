using WeatherDashboard.Domain.Entities;
using WeatherDashboard.Domain.Services;
using Xunit;

namespace WeatherDashboard.Tests.Domain;

public class WeatherStatsCalculatorTests
{
    private static WeatherRecord Record(
        DateTime observedAtUtc, double temp, double min, double max, int humidity, double wind,
        string icon = "01d", string description = "céu limpo", DateTime? sunrise = null, DateTime? sunset = null) => new()
    {
        CityId = "sao-paulo",
        CityName = "São Paulo",
        Uf = "SP",
        ObservedAtUtc = observedAtUtc,
        CollectedAtUtc = observedAtUtc,
        TemperatureC = temp,
        FeelsLikeC = temp,
        TempMinC = min,
        TempMaxC = max,
        HumidityPercent = humidity,
        PressureHPa = 1013,
        WindSpeedMs = wind,
        WeatherMain = "Clear",
        WeatherDescription = description,
        WeatherIcon = icon,
        SunriseUtc = sunrise,
        SunsetUtc = sunset,
    };

    [Fact]
    public void CalculateDailySeries_GroupsReadingsByDayAndAggregatesCorrectly()
    {
        var day1 = new DateTime(2026, 8, 20, 0, 0, 0, DateTimeKind.Utc);
        var day2 = new DateTime(2026, 8, 21, 0, 0, 0, DateTimeKind.Utc);

        var records = new[]
        {
            Record(day1.AddHours(9), temp: 20, min: 18, max: 22, humidity: 60, wind: 2),
            Record(day1.AddHours(15), temp: 26, min: 18, max: 28, humidity: 40, wind: 4),
            Record(day2.AddHours(9), temp: 15, min: 14, max: 17, humidity: 80, wind: 1),
        };

        var series = WeatherStatsCalculator.CalculateDailySeries(
            records, DateOnly.FromDateTime(day1), DateOnly.FromDateTime(day2));

        Assert.Equal(2, series.Count);

        var first = series[0];
        Assert.Equal(DateOnly.FromDateTime(day1), first.Date);
        Assert.Equal(18, first.TempMinC);
        Assert.Equal(28, first.TempMaxC);
        Assert.Equal(23, first.TempAvgC); // média de 20 e 26
        Assert.Equal(50, first.HumidityAvgPercent); // média de 60 e 40
        Assert.Equal(2, first.ReadingsCount);

        var second = series[1];
        Assert.Equal(DateOnly.FromDateTime(day2), second.Date);
        Assert.Equal(1, second.ReadingsCount);
    }

    [Fact]
    public void CalculateDailySeries_UsesIconFromHighestTemperatureReadingAsRepresentative()
    {
        var day = new DateTime(2026, 8, 20, 0, 0, 0, DateTimeKind.Utc);
        var records = new[]
        {
            Record(day.AddHours(7), temp: 17, min: 15, max: 18, humidity: 80, wind: 1, icon: "50d", description: "neblina"),
            Record(day.AddHours(13), temp: 29, min: 15, max: 30, humidity: 40, wind: 2, icon: "01d", description: "céu limpo"),
            Record(day.AddHours(19), temp: 22, min: 15, max: 30, humidity: 55, wind: 1, icon: "02n", description: "poucas nuvens"),
        };

        var series = WeatherStatsCalculator.CalculateDailySeries(records, DateOnly.FromDateTime(day), DateOnly.FromDateTime(day));

        var stats = Assert.Single(series);
        Assert.Equal("01d", stats.RepresentativeIcon);
        Assert.Equal("céu limpo", stats.RepresentativeDescription);
    }

    [Fact]
    public void CalculateDailySeries_ExcludesDaysOutsideRequestedRange()
    {
        var inRange = new DateTime(2026, 8, 20, 12, 0, 0, DateTimeKind.Utc);
        var outOfRange = new DateTime(2026, 8, 25, 12, 0, 0, DateTimeKind.Utc);

        var records = new[]
        {
            Record(inRange, temp: 20, min: 18, max: 22, humidity: 50, wind: 3),
            Record(outOfRange, temp: 30, min: 28, max: 32, humidity: 30, wind: 5),
        };

        var series = WeatherStatsCalculator.CalculateDailySeries(
            records, DateOnly.FromDateTime(inRange), DateOnly.FromDateTime(inRange));

        var day = Assert.Single(series);
        Assert.Equal(DateOnly.FromDateTime(inRange), day.Date);
    }

    [Fact]
    public void CalculateDailySeries_SwapsStartAndEndWhenReversed()
    {
        var day = new DateTime(2026, 8, 20, 12, 0, 0, DateTimeKind.Utc);
        var records = new[] { Record(day, temp: 20, min: 18, max: 22, humidity: 50, wind: 3) };

        var series = WeatherStatsCalculator.CalculateDailySeries(
            records, DateOnly.FromDateTime(day), DateOnly.FromDateTime(day).AddDays(-5));

        Assert.Single(series);
    }

    [Fact]
    public void CalculateTodayStats_ReturnsEmptyWhenNoReadings()
    {
        var stats = WeatherStatsCalculator.CalculateTodayStats(Array.Empty<WeatherRecord>());

        Assert.Null(stats.CurrentTempC);
        Assert.Equal(0, stats.ReadingsCount);
    }

    [Fact]
    public void CalculateTodayStats_UsesLatestReadingAsCurrentConditionAndAggregatesTheRest()
    {
        var baseTime = new DateTime(2026, 8, 25, 8, 0, 0, DateTimeKind.Utc);
        var records = new[]
        {
            Record(baseTime, temp: 18, min: 16, max: 20, humidity: 70, wind: 2),
            Record(baseTime.AddHours(3), temp: 24, min: 16, max: 26, humidity: 55, wind: 3),
            Record(baseTime.AddHours(6), temp: 27, min: 16, max: 28, humidity: 45, wind: 5),
        };

        var stats = WeatherStatsCalculator.CalculateTodayStats(records);

        Assert.Equal(27, stats.CurrentTempC); // leitura mais recente
        Assert.Equal(16, stats.TempMinC);
        Assert.Equal(28, stats.TempMaxC);
        Assert.Equal(3, stats.ReadingsCount);
    }

    [Fact]
    public void CalculateTodayStats_CarriesSunriseAndSunsetFromLatestReading()
    {
        var baseTime = new DateTime(2026, 8, 25, 8, 0, 0, DateTimeKind.Utc);
        var sunrise = new DateTime(2026, 8, 25, 9, 30, 0, DateTimeKind.Utc);
        var sunset = new DateTime(2026, 8, 25, 20, 45, 0, DateTimeKind.Utc);
        var records = new[]
        {
            Record(baseTime, temp: 18, min: 16, max: 20, humidity: 70, wind: 2, sunrise: sunrise, sunset: sunset),
            Record(baseTime.AddHours(3), temp: 24, min: 16, max: 26, humidity: 55, wind: 3, sunrise: sunrise, sunset: sunset),
        };

        var stats = WeatherStatsCalculator.CalculateTodayStats(records);

        Assert.Equal(sunrise, stats.SunriseUtc);
        Assert.Equal(sunset, stats.SunsetUtc);
    }
}
