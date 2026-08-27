using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeatherDashboard.Domain.Entities;
using WeatherDashboard.Domain.Interfaces;

namespace WeatherDashboard.Infrastructure.ExternalServices;

/// <summary>
/// Implementação de <see cref="IWeatherApiClient"/> que consulta o endpoint "Current Weather Data"
/// da OpenWeatherMap (https://openweathermap.org/current). Falhas de rede/API nunca propagam para
/// o chamador: são logadas e resultam em null, para que o coletor em background siga para a
/// próxima capital em vez de derrubar o ciclo inteiro.
/// </summary>
public class OpenWeatherMapClient : IWeatherApiClient
{
    private readonly HttpClient _httpClient;
    private readonly OpenWeatherMapOptions _options;
    private readonly ILogger<OpenWeatherMapClient> _logger;

    public OpenWeatherMapClient(HttpClient httpClient, IOptions<OpenWeatherMapOptions> options, ILogger<OpenWeatherMapClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<WeatherRecord?> GetCurrentWeatherAsync(TrackedCity city, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogWarning("OpenWeatherMap:ApiKey não configurada. Pulando coleta para {City}.", city.City);
            return null;
        }

        var lat = city.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var lon = city.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var url = $"{_options.BaseUrl}?lat={lat}&lon={lon}&appid={_options.ApiKey}&units={_options.Units}&lang={_options.Language}";

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "OpenWeatherMap retornou {StatusCode} para {City}.", response.StatusCode, city.City);
                return null;
            }

            var payload = await response.Content.ReadFromJsonAsync<OpenWeatherMapResponse>(cancellationToken: cancellationToken);
            if (payload?.Main is null)
            {
                _logger.LogWarning("Resposta da OpenWeatherMap sem dados de 'main' para {City}.", city.City);
                return null;
            }

            var weather = payload.Weather.FirstOrDefault();
            var observedAtUtc = payload.Dt > 0
                ? DateTimeOffset.FromUnixTimeSeconds(payload.Dt).UtcDateTime
                : DateTime.UtcNow;

            return new WeatherRecord
            {
                CityId = city.Id,
                CityName = city.City,
                Uf = city.Uf,
                ObservedAtUtc = observedAtUtc,
                CollectedAtUtc = DateTime.UtcNow,
                TemperatureC = payload.Main.Temp,
                FeelsLikeC = payload.Main.FeelsLike,
                TempMinC = payload.Main.TempMin,
                TempMaxC = payload.Main.TempMax,
                HumidityPercent = payload.Main.Humidity,
                PressureHPa = payload.Main.Pressure,
                WindSpeedMs = payload.Wind?.Speed ?? 0,
                WeatherMain = weather?.Main ?? "Unknown",
                WeatherDescription = weather?.Description ?? string.Empty,
                WeatherIcon = weather?.Icon ?? "01d",
                SunriseUtc = payload.Sys?.Sunrise > 0 ? DateTimeOffset.FromUnixTimeSeconds(payload.Sys.Sunrise).UtcDateTime : null,
                SunsetUtc = payload.Sys?.Sunset > 0 ? DateTimeOffset.FromUnixTimeSeconds(payload.Sys.Sunset).UtcDateTime : null,
            };
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        {
            _logger.LogWarning(ex, "Falha ao consultar clima para {City}.", city.City);
            return null;
        }
    }
}
