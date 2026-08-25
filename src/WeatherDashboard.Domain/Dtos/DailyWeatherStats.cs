namespace WeatherDashboard.Domain.Dtos;

/// <summary>Agregação de um dia dentro do período consultado (usada nos gráficos históricos).</summary>
public sealed record DailyWeatherStats(
    DateOnly Date,
    double TempMinC,
    double TempMaxC,
    double TempAvgC,
    double HumidityAvgPercent,
    double WindAvgMs,
    int ReadingsCount);
