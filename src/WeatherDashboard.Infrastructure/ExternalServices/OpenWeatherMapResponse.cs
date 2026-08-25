using System.Text.Json.Serialization;

namespace WeatherDashboard.Infrastructure.ExternalServices;

/// <summary>Subconjunto do payload de /data/2.5/weather relevante para a aplicação.</summary>
internal sealed class OpenWeatherMapResponse
{
    [JsonPropertyName("weather")]
    public List<WeatherInfo> Weather { get; set; } = new();

    [JsonPropertyName("main")]
    public MainInfo? Main { get; set; }

    [JsonPropertyName("wind")]
    public WindInfo? Wind { get; set; }

    [JsonPropertyName("sys")]
    public SysInfo? Sys { get; set; }

    [JsonPropertyName("dt")]
    public long Dt { get; set; }

    internal sealed class WeatherInfo
    {
        [JsonPropertyName("main")]
        public string Main { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("icon")]
        public string Icon { get; set; } = string.Empty;
    }

    internal sealed class MainInfo
    {
        [JsonPropertyName("temp")]
        public double Temp { get; set; }

        [JsonPropertyName("feels_like")]
        public double FeelsLike { get; set; }

        [JsonPropertyName("temp_min")]
        public double TempMin { get; set; }

        [JsonPropertyName("temp_max")]
        public double TempMax { get; set; }

        [JsonPropertyName("pressure")]
        public int Pressure { get; set; }

        [JsonPropertyName("humidity")]
        public int Humidity { get; set; }
    }

    internal sealed class WindInfo
    {
        [JsonPropertyName("speed")]
        public double Speed { get; set; }
    }

    internal sealed class SysInfo
    {
        [JsonPropertyName("sunrise")]
        public long Sunrise { get; set; }

        [JsonPropertyName("sunset")]
        public long Sunset { get; set; }
    }
}
