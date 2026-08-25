namespace WeatherDashboard.Infrastructure.ExternalServices;

/// <summary>Configuração vinda de appsettings.json / variáveis de ambiente / user-secrets (seção "OpenWeatherMap").</summary>
public class OpenWeatherMapOptions
{
    public const string SectionName = "OpenWeatherMap";

    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.openweathermap.org/data/2.5/weather";
    public string Units { get; set; } = "metric";
    public string Language { get; set; } = "pt_br";
}
