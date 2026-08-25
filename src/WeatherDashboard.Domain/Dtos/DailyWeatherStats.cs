namespace WeatherDashboard.Domain.Dtos;

/// <summary>Agregação de um dia dentro do período consultado (usada nos gráficos e na tira de dias).</summary>
public sealed record DailyWeatherStats(
    DateOnly Date,
    double TempMinC,
    double TempMaxC,
    double TempAvgC,
    double HumidityAvgPercent,
    double WindAvgMs,
    int ReadingsCount,
    string RepresentativeIcon,
    string RepresentativeDescription);
