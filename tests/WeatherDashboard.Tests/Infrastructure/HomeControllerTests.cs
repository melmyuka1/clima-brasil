using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WeatherDashboard.Domain.Entities;
using WeatherDashboard.Domain.Interfaces;
using WeatherDashboard.Web.Controllers;
using WeatherDashboard.Web.Models;
using Xunit;

namespace WeatherDashboard.Tests.Infrastructure;

public class HomeControllerTests
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

    private static HomeController CreateController(Mock<IWeatherRecordRepository> repositoryMock) =>
        new(repositoryMock.Object, NullLogger<HomeController>.Instance);

    [Fact]
    public void Index_ReturnsDefaultCapitalWhenNoneSelected()
    {
        var controller = CreateController(new Mock<IWeatherRecordRepository>());

        var result = Assert.IsType<ViewResult>(controller.Index(city: null));
        var model = Assert.IsType<DashboardIndexViewModel>(result.Model);

        Assert.Equal("curitiba", model.SelectedCityId);
        Assert.Equal(27, model.Capitals.Count);
    }

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

        var jsonResult = Assert.IsType<JsonResult>(result);
        var response = Assert.IsType<DashboardDataResponse>(jsonResult.Value);
        Assert.Equal("São Paulo", response.CityName);
        Assert.Single(response.DailySeries);
        Assert.Equal(24, response.Today.CurrentTempC);
    }
}
