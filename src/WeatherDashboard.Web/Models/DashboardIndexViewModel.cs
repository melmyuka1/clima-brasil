using WeatherDashboard.Domain.Entities;

namespace WeatherDashboard.Web.Models;

/// <summary>Dados necessários para renderizar a página inicial (o histórico em si chega via AJAX).</summary>
public sealed class DashboardIndexViewModel
{
    public required IReadOnlyList<TrackedCity> MetroCities { get; init; }
    public required IReadOnlyList<TrackedCity> Capitals { get; init; }
    public required string SelectedCityId { get; init; }
    public required DateOnly DefaultStart { get; init; }
    public required DateOnly DefaultEnd { get; init; }
}
