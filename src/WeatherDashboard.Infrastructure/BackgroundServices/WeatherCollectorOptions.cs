namespace WeatherDashboard.Infrastructure.BackgroundServices;

/// <summary>Configuração do coletor periódico (seção "WeatherCollector").</summary>
public class WeatherCollectorOptions
{
    public const string SectionName = "WeatherCollector";

    /// <summary>Intervalo entre coletas, em minutos. Requisito do exercício: 15 minutos.</summary>
    public int IntervalMinutes { get; set; } = 15;
}
