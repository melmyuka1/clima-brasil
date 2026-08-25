namespace WeatherDashboard.Domain.Entities;

/// <summary>
/// Uma capital de estado brasileira. Catálogo estático usado tanto para a coleta
/// periódica de dados quanto para popular o seletor de cidades no dashboard.
/// </summary>
public sealed class BrazilianCapital
{
    public required string Id { get; init; }
    public required string City { get; init; }
    public required string Uf { get; init; }
    public required string State { get; init; }
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
}
