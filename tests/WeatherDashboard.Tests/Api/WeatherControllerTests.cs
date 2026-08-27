using Microsoft.AspNetCore.Mvc;
using Moq;
using WeatherDashboard.Api.Controllers;
using WeatherDashboard.Api.Models;
using WeatherDashboard.Domain.Entities;
using WeatherDashboard.Domain.Interfaces;
using Xunit;

namespace WeatherDashboard.Tests.Api;

public class WeatherControllerTests
{
    private static WeatherRecord Record(DateTime observedAtUtc, double temp) => new()
    {
        CityId = "sao-paulo",
        CityName = "São Paulo",
        Uf = "SP",
        ObservedAtUtc = observedAtUtc,
        CollectedAtUtc = observedAtUtc,
        TemperatureC = temp,
        FeelsLikeC = temp,
        TempMinC = temp - 2,
        TempMaxC = temp + 2,
        HumidityPercent = 55,
        PressureHPa = 1013,
        WindSpeedMs = 3,
        WeatherMain = "Clear",
        WeatherDescription = "céu limpo",
        WeatherIcon = "01d",
    };

    private static WeatherController CreateController(Mock<IWeatherRecordRepository> repositoryMock) =>
        new(repositoryMock.Object);

    [Fact]
    public async Task Data_ReturnsBadRequestForUnknownCity()
    {
        var controller = CreateController(new Mock<IWeatherRecordRepository>());

        var result = await controller.Data("atlantis", new DateOnly(2026, 8, 20), new DateOnly(2026, 8, 25), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Data_ReturnsBadRequestWhenEndBeforeStart()
    {
        var controller = CreateController(new Mock<IWeatherRecordRepository>());

        var result = await controller.Data("sao-paulo", new DateOnly(2026, 8, 25), new DateOnly(2026, 8, 20), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Data_ReturnsAggregatedSeriesAndTodayStatsForKnownCity()
    {
        var repositoryMock = new Mock<IWeatherRecordRepository>();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todayReading = Record(DateTime.UtcNow, 24);

        repositoryMock
            .Setup(r => r.GetByCityAndRangeAsync("sao-paulo", today.AddDays(-1), today, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WeatherRecord> { todayReading });

        var controller = CreateController(repositoryMock);

        var result = await controller.Data("sao-paulo", today.AddDays(-1), today, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<DashboardDataResponse>(okResult.Value);
        Assert.Equal("São Paulo", response.CityName);
        Assert.Single(response.DailySeries);
        Assert.Equal(24, response.Today.CurrentTempC);
    }

    [Fact]
    public async Task Highlights_ReturnsMetroBorderCitiesAndCuratedCapitals()
    {
        var repositoryMock = new Mock<IWeatherRecordRepository>();
        repositoryMock
            .Setup(r => r.GetByCityAndRangeAsync(It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WeatherRecord>());

        var controller = CreateController(repositoryMock);

        var result = await controller.Highlights(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<HighlightsResponse>(okResult.Value);
        Assert.Equal(9, response.MetroCities.Count);
        Assert.Equal("curitiba", response.MetroCities[0].CityId);
        Assert.Equal(8, response.Capitals.Count);
    }
}
