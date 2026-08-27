using WeatherDashboard.Domain.Dtos;

namespace WeatherDashboard.Api.Models;

/// <summary>Série histórica agregada por dia e estatísticas do dia atual para uma cidade e período.</summary>
public sealed class DashboardDataResponse
{
    public required string CityId { get; init; }
    public required string CityName { get; init; }
    public required string Uf { get; init; }
    public required DateOnly Start { get; init; }
    public required DateOnly End { get; init; }
    public required IReadOnlyList<DailyWeatherStats> DailySeries { get; init; }
    public required TodayWeatherStats Today { get; init; }
}
