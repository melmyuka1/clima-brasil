namespace WeatherDashboard.Domain.Entities;

/// <summary>
/// Uma cidade rastreada pela aplicação (capital de estado ou município da região
/// metropolitana de Curitiba). Catálogo estático usado tanto para a coleta periódica
/// de dados quanto para popular os seletores de cidade no dashboard.
/// </summary>
public sealed class TrackedCity
{
    public required string Id { get; init; }
    public required string City { get; init; }
    public required string Uf { get; init; }
    public required string State { get; init; }
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
}
