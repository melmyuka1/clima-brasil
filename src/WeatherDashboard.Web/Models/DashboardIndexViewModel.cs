using WeatherDashboard.Domain.Entities;

namespace WeatherDashboard.Web.Models;

/// <summary>
/// Dados necessários para renderizar a página inicial. O histórico climático em si chega via
/// fetch do navegador direto para <see cref="ApiBaseUrl"/> (WeatherDashboard.Api).
/// </summary>
public sealed class DashboardIndexViewModel
{
    public required IReadOnlyList<TrackedCity> MetroCities { get; init; }
    public required IReadOnlyList<TrackedCity> Capitals { get; init; }
    public required string SelectedCityId { get; init; }
    public required DateOnly DefaultStart { get; init; }
    public required DateOnly DefaultEnd { get; init; }
    public required string ApiBaseUrl { get; init; }
}
