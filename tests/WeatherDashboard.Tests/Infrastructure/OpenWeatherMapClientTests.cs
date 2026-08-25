using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using WeatherDashboard.Domain.Catalog;
using WeatherDashboard.Infrastructure.ExternalServices;
using Xunit;

namespace WeatherDashboard.Tests.Infrastructure;

public class OpenWeatherMapClientTests
{
    private static readonly OpenWeatherMapOptions ValidOptions = new()
    {
        ApiKey = "fake-key",
        BaseUrl = "https://api.openweathermap.org/data/2.5/weather",
        Units = "metric",
        Language = "pt_br",
    };

    private const string SampleJson = """
        {
          "weather": [{ "main": "Clear", "description": "céu limpo", "icon": "01d" }],
          "main": { "temp": 25.5, "feels_like": 25.1, "temp_min": 23.0, "temp_max": 27.0, "pressure": 1013, "humidity": 60 },
          "wind": { "speed": 3.6 },
          "dt": 1735707600
        }
        """;

    private static OpenWeatherMapClient CreateClient(HttpMessageHandler handler, OpenWeatherMapOptions? options = null)
    {
        var httpClient = new HttpClient(handler);
        return new OpenWeatherMapClient(httpClient, Options.Create(options ?? ValidOptions), NullLogger<OpenWeatherMapClient>.Instance);
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_ParsesSuccessfulResponseIntoWeatherRecord()
    {
        var client = CreateClient(new FakeHttpMessageHandler(HttpStatusCode.OK, SampleJson));

        var record = await client.GetCurrentWeatherAsync(BrazilianCapitals.Default);

        Assert.NotNull(record);
        Assert.Equal("sao-paulo", record!.CityId);
        Assert.Equal(25.5, record.TemperatureC);
        Assert.Equal(60, record.HumidityPercent);
        Assert.Equal(3.6, record.WindSpeedMs);
        Assert.Equal("céu limpo", record.WeatherDescription);
        Assert.Equal("01d", record.WeatherIcon);
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_ReturnsNullWhenApiKeyIsMissing()
    {
        var client = CreateClient(
            new FakeHttpMessageHandler(HttpStatusCode.OK, SampleJson),
            new OpenWeatherMapOptions { ApiKey = "" });

        var record = await client.GetCurrentWeatherAsync(BrazilianCapitals.Default);

        Assert.Null(record);
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_ReturnsNullOnHttpErrorStatus()
    {
        var client = CreateClient(new FakeHttpMessageHandler(HttpStatusCode.Unauthorized, null));

        var record = await client.GetCurrentWeatherAsync(BrazilianCapitals.Default);

        Assert.Null(record);
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_ReturnsNullWhenRequestThrows()
    {
        var client = CreateClient(new FakeHttpMessageHandler(new HttpRequestException("boom")));

        var record = await client.GetCurrentWeatherAsync(BrazilianCapitals.Default);

        Assert.Null(record);
    }
}
