namespace WeatherDashboard.Domain.Dtos;

/// <summary>Estatísticas do dia corrente, exibidas nos cartões do dashboard.</summary>
public sealed record TodayWeatherStats(
    double? CurrentTempC,
    double? CurrentFeelsLikeC,
    string? CurrentDescription,
    string? CurrentIcon,
    double? TempMinC,
    double? TempMaxC,
    double? TempAvgC,
    double? HumidityAvgPercent,
    double? WindAvgMs,
    int ReadingsCount,
    DateTime? LastUpdatedUtc,
    DateTime? SunriseUtc,
    DateTime? SunsetUtc)
{
    public static readonly TodayWeatherStats Empty = new(null, null, null, null, null, null, null, null, null, 0, null, null, null);
}
