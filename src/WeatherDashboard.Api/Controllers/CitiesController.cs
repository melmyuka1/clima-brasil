using Microsoft.AspNetCore.Mvc;
using WeatherDashboard.Api.Models;
using WeatherDashboard.Domain.Catalog;
using WeatherDashboard.Domain.Entities;

namespace WeatherDashboard.Api.Controllers;

/// <summary>Catálogos estáticos de cidades rastreadas (não dependem de banco de dados).</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CitiesController : ControllerBase
{
    /// <summary>Os 29 municípios da Região Metropolitana de Curitiba.</summary>
    [HttpGet("metro-region")]
    [ProducesResponseType(typeof(IReadOnlyList<TrackedCityResponse>), StatusCodes.Status200OK)]
    public IActionResult MetroRegion() => Ok(Map(CuritibaMetroRegion.All));

    /// <summary>Os municípios da RMC que fazem fronteira direta com Curitiba (subconjunto de <c>metro-region</c>).</summary>
    [HttpGet("metro-region/border")]
    [ProducesResponseType(typeof(IReadOnlyList<TrackedCityResponse>), StatusCodes.Status200OK)]
    public IActionResult MetroRegionBorder() => Ok(Map(CuritibaMetroRegion.BorderCities));

    /// <summary>As 27 capitais estaduais brasileiras (26 estados + Distrito Federal).</summary>
    [HttpGet("capitals")]
    [ProducesResponseType(typeof(IReadOnlyList<TrackedCityResponse>), StatusCodes.Status200OK)]
    public IActionResult Capitals() => Ok(Map(BrazilianCapitals.All));

    /// <summary>União de todas as cidades rastreadas pelo coletor em background (capitais + RMC, sem duplicar Curitiba).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TrackedCityResponse>), StatusCodes.Status200OK)]
    public IActionResult All() => Ok(Map(TrackedCities.All));

    private static IReadOnlyList<TrackedCityResponse> Map(IEnumerable<TrackedCity> cities) => cities
        .Select(c => new TrackedCityResponse
        {
            Id = c.Id,
            City = c.City,
            Uf = c.Uf,
            State = c.State,
            Latitude = c.Latitude,
            Longitude = c.Longitude,
        })
        .ToList();
}
