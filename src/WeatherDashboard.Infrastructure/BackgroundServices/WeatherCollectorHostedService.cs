using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeatherDashboard.Domain.Catalog;
using WeatherDashboard.Domain.Interfaces;

namespace WeatherDashboard.Infrastructure.BackgroundServices;

/// <summary>
/// Serviço em background que, a cada N minutos (padrão: 15), consulta a condição climática
/// atual de todas as capitais brasileiras e grava cada leitura como um novo registro histórico.
/// Roda uma coleta imediatamente na inicialização para que o dashboard já tenha dados ao subir.
/// </summary>
public class WeatherCollectorHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly WeatherCollectorOptions _options;
    private readonly ILogger<WeatherCollectorHostedService> _logger;

    public WeatherCollectorHostedService(
        IServiceScopeFactory scopeFactory,
        IOptions<WeatherCollectorOptions> options,
        ILogger<WeatherCollectorHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromMinutes(Math.Max(1, _options.IntervalMinutes));
        using var timer = new PeriodicTimer(interval);

        do
        {
            await CollectAllCapitalsAsync(stoppingToken);
        }
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task CollectAllCapitalsAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando ciclo de coleta climática para {Count} capitais.", BrazilianCapitals.All.Count);

        using var scope = _scopeFactory.CreateScope();
        var apiClient = scope.ServiceProvider.GetRequiredService<IWeatherApiClient>();
        var repository = scope.ServiceProvider.GetRequiredService<IWeatherRecordRepository>();

        var collected = 0;
        foreach (var capital in BrazilianCapitals.All)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                var record = await apiClient.GetCurrentWeatherAsync(capital, stoppingToken);
                if (record is not null)
                {
                    await repository.AddAsync(record, stoppingToken);
                    collected++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao coletar clima de {City}.", capital.City);
            }
        }

        _logger.LogInformation("Ciclo de coleta concluído: {Collected}/{Total} capitais.", collected, BrazilianCapitals.All.Count);
    }
}
