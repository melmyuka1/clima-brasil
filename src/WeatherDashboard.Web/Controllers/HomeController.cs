using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WeatherDashboard.Domain.Catalog;
using WeatherDashboard.Domain.Entities;
using WeatherDashboard.Domain.Interfaces;
using WeatherDashboard.Domain.Services;
using WeatherDashboard.Web.Models;

namespace WeatherDashboard.Web.Controllers;

public class HomeController : Controller
{
    /// <summary>Capitais exibidas quando o usuário alterna a tira de destaque para "Ver capitais". Curitiba sempre em primeiro.</summary>
    private static readonly string[] HighlightCapitalIds =
    {
        "curitiba", "sao-paulo", "rio-de-janeiro", "brasilia", "salvador", "manaus", "porto-alegre", "florianopolis",
    };

    private readonly IWeatherRecordRepository _repository;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IWeatherRecordRepository repository, ILogger<HomeController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>Página inicial: dashboard com os seletores de cidade e os filtros de data.</summary>
    public IActionResult Index(string? city)
    {
        var selected = (city is not null ? TrackedCities.FindById(city) : null) ?? TrackedCities.Default;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var viewModel = new DashboardIndexViewModel
        {
            MetroCities = CuritibaMetroRegion.All,
            Capitals = BrazilianCapitals.All,
            SelectedCityId = selected.Id,
            DefaultStart = today.AddDays(-6),
            DefaultEnd = today,
        };

        return View(viewModel);
    }

    /// <summary>
    /// Endpoint JSON consumido via fetch pelo dashboard: série histórica agregada por dia (para
    /// os gráficos) e estatísticas do dia atual (para os cartões), filtrados por cidade e período.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Data(string city, DateOnly start, DateOnly end, CancellationToken cancellationToken)
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

        var response = new DashboardDataResponse
        {
            CityId = selected.Id,
            CityName = selected.City,
            Uf = selected.Uf,
            Start = start,
            End = end,
            DailySeries = dailySeries,
            Today = todayStats,
        };

        return Json(response);
    }

    /// <summary>
    /// Leitura atual (hoje) das cidades em destaque, para a tira de atalhos sob o cabeçalho — por
    /// padrão os municípios da Região Metropolitana de Curitiba que fazem fronteira com a capital;
    /// o botão "Ver capitais" alterna, no cliente, para o conjunto de capitais estaduais aqui
    /// devolvido junto. Os dois conjuntos vêm numa única resposta para não exigir uma nova
    /// requisição a cada alternância.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Highlights(CancellationToken cancellationToken)
    {
        var metroCities = await BuildHighlightsAsync(CuritibaMetroRegion.BorderCities, cancellationToken);
        var capitals = await BuildHighlightsAsync(
            HighlightCapitalIds.Select(id => BrazilianCapitals.FindById(id)).Where(c => c is not null)!,
            cancellationToken);

        return Json(new HighlightsResponse { MetroCities = metroCities, Capitals = capitals });
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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var model = new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier };
        return View(model);
    }
}
