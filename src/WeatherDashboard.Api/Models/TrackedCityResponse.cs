namespace WeatherDashboard.Api.Models;

/// <summary>Um item do catálogo de cidades rastreadas.</summary>
public sealed class TrackedCityResponse
{
    public required string Id { get; init; }
    public required string City { get; init; }
    public required string Uf { get; init; }
    public required string State { get; init; }
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
}
