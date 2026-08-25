namespace WeatherDashboard.Web.Models;

/// <summary>Um item da tira de "capitais em destaque" exibida sob o cabeçalho do dashboard.</summary>
public sealed class HighlightCityResponse
{
    public required string CityId { get; init; }
    public required string CityName { get; init; }
    public required string Uf { get; init; }
    public double? CurrentTempC { get; init; }
    public string? Icon { get; init; }
    public string? Description { get; init; }
}
