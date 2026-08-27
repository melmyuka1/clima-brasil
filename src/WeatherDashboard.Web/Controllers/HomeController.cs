using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WeatherDashboard.Domain.Catalog;
using WeatherDashboard.Web.Models;

namespace WeatherDashboard.Web.Controllers;

/// <summary>
/// Controller puramente de apresentação: monta a página inicial (catálogo de cidades para os
/// seletores + a URL base da API) e a página de erro. O histórico climático em si é buscado pelo
/// navegador diretamente de <c>WeatherDashboard.Api</c> (ver wwwroot/js/dashboard.js).
/// </summary>
public class HomeController : Controller
{
    private readonly IConfiguration _configuration;

    public HomeController(IConfiguration configuration)
    {
        _configuration = configuration;
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
            ApiBaseUrlHttp = _configuration["WeatherApi:BaseUrlHttp"] ?? "http://localhost:5282",
            ApiBaseUrlHttps = _configuration["WeatherApi:BaseUrlHttps"] ?? "https://localhost:7222",
        };

        return View(viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var model = new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier };
        return View(model);
    }
}
