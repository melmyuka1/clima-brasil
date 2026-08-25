using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WeatherDashboard.Domain.Catalog;
using WeatherDashboard.Domain.Interfaces;
using WeatherDashboard.Domain.Services;
using WeatherDashboard.Web.Models;

namespace WeatherDashboard.Web.Controllers;

public class HomeController : Controller
{
    /// <summary>Capitais exibidas na tira de destaque sob o cabeçalho. Curitiba sempre em primeiro.</summary>
    private static readonly string[] HighlightCityIds =
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

    /// <summary>Página inicial: dashboard com o seletor de capitais e os filtros de data.</summary>
    public IActionResult Index(string? city)
    {
        var capital = (city is not null ? BrazilianCapitals.FindById(city) : null) ?? BrazilianCapitals.Default;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var viewModel = new DashboardIndexViewModel
        {
            Capitals = BrazilianCapitals.All,
            SelectedCityId = capital.Id,
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
        var capital = BrazilianCapitals.FindById(city);
        if (capital is null)
        {
            return BadRequest(new { error = $"Cidade '{city}' não encontrada no catálogo de capitais." });
        }

        if (end < start)
        {
            return BadRequest(new { error = "A data final não pode ser anterior à data inicial." });
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var rangeRecords = await _repository.GetByCityAndRangeAsync(capital.Id, start, end, cancellationToken);
        var dailySeries = WeatherStatsCalculator.CalculateDailySeries(rangeRecords, start, end);

        // As estatísticas "do dia atual" usam sempre a data corrente, independentemente do filtro escolhido.
        var todayRecords = start <= today && today <= end
            ? rangeRecords.Where(r => DateOnly.FromDateTime(r.ObservedAtUtc) == today)
            : await _repository.GetByCityAndRangeAsync(capital.Id, today, today, cancellationToken);
        var todayStats = WeatherStatsCalculator.CalculateTodayStats(todayRecords);

        var response = new DashboardDataResponse
        {
            CityId = capital.Id,
            CityName = capital.City,
            Uf = capital.Uf,
            Start = start,
            End = end,
            DailySeries = dailySeries,
            Today = todayStats,
        };

        return Json(response);
    }

    /// <summary>
    /// Leitura atual (hoje) de um punhado de capitais em destaque, para a tira de atalhos sob o
    /// cabeçalho — o equivalente, neste catálogo fixo de capitais, às "cidades salvas" de um app
    /// de clima comum.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Highlights(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var results = new List<HighlightCityResponse>();
        foreach (var cityId in HighlightCityIds)
        {
            var capital = BrazilianCapitals.FindById(cityId);
            if (capital is null)
            {
                continue;
            }

            var records = await _repository.GetByCityAndRangeAsync(capital.Id, today, today, cancellationToken);
            var todayStats = WeatherStatsCalculator.CalculateTodayStats(records);

            results.Add(new HighlightCityResponse
            {
                CityId = capital.Id,
                CityName = capital.City,
                Uf = capital.Uf,
                CurrentTempC = todayStats.CurrentTempC,
                Icon = todayStats.CurrentIcon,
                Description = todayStats.CurrentDescription,
            });
        }

        return Json(results);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var model = new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier };
        return View(model);
    }
}
