namespace WeatherDashboard.Web.Models;

/// <summary>Um item da tira de destaque exibida sob o cabeçalho do dashboard.</summary>
public sealed class HighlightCityResponse
{
    public required string CityId { get; init; }
    public required string CityName { get; init; }
    public required string Uf { get; init; }
    public double? CurrentTempC { get; init; }
    public string? Icon { get; init; }
    public string? Description { get; init; }
}

/// <summary>
/// Os dois conjuntos que a tira de destaque pode exibir: a Região Metropolitana de Curitiba
/// (padrão ao abrir o dashboard) e as capitais estaduais (alternadas pelo botão "Ver capitais").
/// </summary>
public sealed class HighlightsResponse
{
    public required IReadOnlyList<HighlightCityResponse> MetroCities { get; init; }
    public required IReadOnlyList<HighlightCityResponse> Capitals { get; init; }
}
