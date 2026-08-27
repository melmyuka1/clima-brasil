using Microsoft.AspNetCore.Mvc;
using WeatherDashboard.Api.Controllers;
using WeatherDashboard.Api.Models;
using Xunit;

namespace WeatherDashboard.Tests.Api;

public class CitiesControllerTests
{
    private static readonly CitiesController Controller = new();

    [Fact]
    public void MetroRegion_ReturnsAllTwentyNineMunicipalities()
    {
        var result = Assert.IsType<OkObjectResult>(Controller.MetroRegion());
        var cities = Assert.IsAssignableFrom<IReadOnlyList<TrackedCityResponse>>(result.Value);

        Assert.Equal(29, cities.Count);
    }

    [Fact]
    public void MetroRegionBorder_ReturnsTheNineBorderMunicipalities()
    {
        var result = Assert.IsType<OkObjectResult>(Controller.MetroRegionBorder());
        var cities = Assert.IsAssignableFrom<IReadOnlyList<TrackedCityResponse>>(result.Value);

        Assert.Equal(9, cities.Count);
        Assert.Contains(cities, c => c.Id == "curitiba");
    }

    [Fact]
    public void Capitals_ReturnsAllTwentySevenCapitals()
    {
        var result = Assert.IsType<OkObjectResult>(Controller.Capitals());
        var cities = Assert.IsAssignableFrom<IReadOnlyList<TrackedCityResponse>>(result.Value);

        Assert.Equal(27, cities.Count);
    }

    [Fact]
    public void All_ReturnsTheDeduplicatedUnion()
    {
        var result = Assert.IsType<OkObjectResult>(Controller.All());
        var cities = Assert.IsAssignableFrom<IReadOnlyList<TrackedCityResponse>>(result.Value);

        Assert.Equal(27 + 29 - 1, cities.Count);
    }
}
