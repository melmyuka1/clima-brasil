using Microsoft.AspNetCore.Mvc;
using WeatherDashboard.Api.Models;
using WeatherDashboard.Domain.Catalog;
using WeatherDashboard.Domain.Entities;
using WeatherDashboard.Domain.Interfaces;
using WeatherDashboard.Domain.Services;

namespace WeatherDashboard.Api.Controllers;

/// <summary>Histórico climático agregado e destaques, para consumo do site (WeatherDashboard.Web) ou de qualquer outro cliente.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class WeatherController : ControllerBase
{
    /// <summary>Capitais exibidas quando o cliente alterna a tira de destaque para "Ver capitais". Curitiba sempre em primeiro.</summary>
    private static readonly string[] HighlightCapitalIds =
    {
        "curitiba", "sao-paulo", "rio-de-janeiro", "brasilia", "salvador", "manaus", "porto-alegre", "florianopolis",
    };

    private readonly IWeatherRecordRepository _repository;

    public WeatherController(IWeatherRecordRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Série histórica agregada por dia (mín/média/máx de temperatura, umidade e vento) para o
    /// período informado, mais as estatísticas do dia atual — sempre a data corrente, independente
    /// do período filtrado.
    /// </summary>
    /// <param name="city">Id da cidade (ver <c>GET /api/cities</c>), ex.: "curitiba".</param>
    /// <param name="start">Data inicial do período (yyyy-MM-dd).</param>
    /// <param name="end">Data final do período (yyyy-MM-dd), não pode ser anterior a <paramref name="start"/>.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    [HttpGet("data")]
    [ProducesResponseType(typeof(DashboardDataResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Data([FromQuery] string city, [FromQuery] DateOnly start, [FromQuery] DateOnly end, CancellationToken cancellationToken)
    {
        var selected = TrackedCities.FindById(city);
        if (selected is null)
        {
            return BadRequest(new { error = $"Cidade '{city}' não encontrada no catálogo de cidades rastreadas." });
        }

        if (end < start)
        {
            return BadRequest(new { error = "A data final não pode ser anterior à data inicial." });
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var rangeRecords = await _repository.GetByCityAndRangeAsync(selected.Id, start, end, cancellationToken);
        var dailySeries = WeatherStatsCalculator.CalculateDailySeries(rangeRecords, start, end);

        // As estatísticas "do dia atual" usam sempre a data corrente, independentemente do filtro escolhido.
        var todayRecords = start <= today && today <= end
            ? rangeRecords.Where(r => DateOnly.FromDateTime(r.ObservedAtUtc) == today)
            : await _repository.GetByCityAndRangeAsync(selected.Id, today, today, cancellationToken);
        var todayStats = WeatherStatsCalculator.CalculateTodayStats(todayRecords);

        return Ok(new DashboardDataResponse
        {
            CityId = selected.Id,
            CityName = selected.City,
            Uf = selected.Uf,
            Start = start,
            End = end,
            DailySeries = dailySeries,
            Today = todayStats,
        });
    }

    /// <summary>
    /// Leitura atual (hoje) das cidades em destaque — por padrão os municípios da Região
    /// Metropolitana de Curitiba que fazem fronteira com a capital, e as capitais estaduais em
    /// destaque, num único conjunto devolvido junto para não exigir uma requisição a cada alternância.
    /// </summary>
    [HttpGet("highlights")]
    [ProducesResponseType(typeof(HighlightsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Highlights(CancellationToken cancellationToken)
    {
        var metroCities = await BuildHighlightsAsync(CuritibaMetroRegion.BorderCities, cancellationToken);
        var capitals = await BuildHighlightsAsync(
            HighlightCapitalIds.Select(id => BrazilianCapitals.FindById(id)).Where(c => c is not null)!,
            cancellationToken);

        return Ok(new HighlightsResponse { MetroCities = metroCities, Capitals = capitals });
    }

    private async Task<List<HighlightCityResponse>> BuildHighlightsAsync(IEnumerable<TrackedCity> cities, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var results = new List<HighlightCityResponse>();

        foreach (var city in cities)
        {
            var records = await _repository.GetByCityAndRangeAsync(city.Id, today, today, cancellationToken);
            var todayStats = WeatherStatsCalculator.CalculateTodayStats(records);

            results.Add(new HighlightCityResponse
            {
                CityId = city.Id,
                CityName = city.City,
                Uf = city.Uf,
                CurrentTempC = todayStats.CurrentTempC,
                Icon = todayStats.CurrentIcon,
                Description = todayStats.CurrentDescription,
            });
        }

        return results;
    }
}
