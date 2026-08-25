using WeatherDashboard.Domain.Dtos;
using WeatherDashboard.Domain.Entities;

namespace WeatherDashboard.Domain.Services;

/// <summary>
/// Regras puras de agregação de leituras climáticas em estatísticas para o dashboard.
/// Não depende de banco de dados, HTTP ou relógio de sistema (a data "hoje" é recebida
/// por parâmetro), o que a torna trivial de testar unitariamente.
/// </summary>
public static class WeatherStatsCalculator
{
    public static IReadOnlyList<DailyWeatherStats> CalculateDailySeries(
        IEnumerable<WeatherRecord> records, DateOnly start, DateOnly end)
    {
        if (end < start)
        {
            (start, end) = (end, start);
        }

        return records
            .GroupBy(r => DateOnly.FromDateTime(r.ObservedAtUtc))
            .Where(g => g.Key >= start && g.Key <= end)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                // A leitura de temperatura mais alta do dia costuma cair perto do meio-dia,
                // então usamos sua condição/ícone como "representante" visual do dia inteiro.
                var representative = g.OrderByDescending(r => r.TemperatureC).First();
                return new DailyWeatherStats(
                    g.Key,
                    TempMinC: g.Min(r => r.TempMinC),
                    TempMaxC: g.Max(r => r.TempMaxC),
                    TempAvgC: Math.Round(g.Average(r => r.TemperatureC), 1),
                    HumidityAvgPercent: Math.Round(g.Average(r => r.HumidityPercent), 1),
                    WindAvgMs: Math.Round(g.Average(r => r.WindSpeedMs), 1),
                    ReadingsCount: g.Count(),
                    RepresentativeIcon: representative.WeatherIcon,
                    RepresentativeDescription: representative.WeatherDescription);
            })
            .ToList();
    }

    public static TodayWeatherStats CalculateTodayStats(IEnumerable<WeatherRecord> todayRecords)
    {
        var list = todayRecords.OrderBy(r => r.ObservedAtUtc).ToList();
        if (list.Count == 0)
        {
            return TodayWeatherStats.Empty;
        }

        var latest = list[^1];
        return new TodayWeatherStats(
            CurrentTempC: latest.TemperatureC,
            CurrentFeelsLikeC: latest.FeelsLikeC,
            CurrentDescription: latest.WeatherDescription,
            CurrentIcon: latest.WeatherIcon,
            TempMinC: list.Min(r => r.TempMinC),
            TempMaxC: list.Max(r => r.TempMaxC),
            TempAvgC: Math.Round(list.Average(r => r.TemperatureC), 1),
            HumidityAvgPercent: Math.Round(list.Average(r => r.HumidityPercent), 1),
            WindAvgMs: Math.Round(list.Average(r => r.WindSpeedMs), 1),
            ReadingsCount: list.Count,
            LastUpdatedUtc: latest.CollectedAtUtc,
            SunriseUtc: latest.SunriseUtc,
            SunsetUtc: latest.SunsetUtc);
    }
}
