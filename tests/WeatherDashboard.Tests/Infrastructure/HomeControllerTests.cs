using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using WeatherDashboard.Web.Controllers;
using WeatherDashboard.Web.Models;
using Xunit;

namespace WeatherDashboard.Tests.Infrastructure;

public class HomeControllerTests
{
    private static HomeController CreateController(string? apiBaseUrl = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(apiBaseUrl is null
                ? []
                : new Dictionary<string, string?> { ["WeatherApi:BaseUrl"] = apiBaseUrl })
            .Build();

        return new HomeController(configuration);
    }

    [Fact]
    public void Index_DefaultsToCuritibaAndExposesBothCityCatalogs()
    {
        var controller = CreateController();

        var result = Assert.IsType<ViewResult>(controller.Index(city: null));
        var model = Assert.IsType<DashboardIndexViewModel>(result.Model);

        Assert.Equal("curitiba", model.SelectedCityId);
        Assert.Equal(27, model.Capitals.Count);
        Assert.Equal(29, model.MetroCities.Count);
    }

    [Fact]
    public void Index_ResolvesSelectedCityFromMetroRegionCatalog()
    {
        var controller = CreateController();

        var result = Assert.IsType<ViewResult>(controller.Index(city: "colombo"));
        var model = Assert.IsType<DashboardIndexViewModel>(result.Model);

        Assert.Equal("colombo", model.SelectedCityId);
    }

    [Fact]
    public void Index_PassesConfiguredApiBaseUrlToTheView()
    {
        var controller = CreateController(apiBaseUrl: "https://api.exemplo.com");

        var result = Assert.IsType<ViewResult>(controller.Index(city: null));
        var model = Assert.IsType<DashboardIndexViewModel>(result.Model);

        Assert.Equal("https://api.exemplo.com", model.ApiBaseUrl);
    }
}
