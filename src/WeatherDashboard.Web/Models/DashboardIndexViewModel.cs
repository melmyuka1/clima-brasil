using WeatherDashboard.Domain.Entities;

namespace WeatherDashboard.Web.Models;

/// <summary>
/// Dados necessários para renderizar a página inicial. O histórico climático em si chega via
/// fetch do navegador direto para a <c>WeatherDashboard.Api</c>. Duas URLs (http/https) são
/// passadas para o JS escolher a que casa com o protocolo da própria página — misturar http e
/// https entre site e API é bloqueado por alguns navegadores (ex.: Firefox) como conteúdo misto.
/// </summary>
public sealed class DashboardIndexViewModel
{
    public required IReadOnlyList<TrackedCity> MetroCities { get; init; }
    public required IReadOnlyList<TrackedCity> Capitals { get; init; }
    public required string SelectedCityId { get; init; }
    public required DateOnly DefaultStart { get; init; }
    public required DateOnly DefaultEnd { get; init; }
    public required string ApiBaseUrlHttp { get; init; }
    public required string ApiBaseUrlHttps { get; init; }
}
