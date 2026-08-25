namespace WeatherDashboard.Domain.Entities;

/// <summary>
/// Uma leitura climática coletada da API externa e persistida no banco de dados.
/// Cada execução do coletor em background grava um registro por capital.
/// </summary>
public sealed class WeatherRecord
{
    public int Id { get; set; }

    public required string CityId { get; set; }
    public required string CityName { get; set; }
    public required string Uf { get; set; }

    /// <summary>Momento (UTC) em que a leitura foi observada pela estação/API de origem.</summary>
    public DateTime ObservedAtUtc { get; set; }

    /// <summary>Momento (UTC) em que o sistema efetivamente gravou o registro.</summary>
    public DateTime CollectedAtUtc { get; set; }

    public double TemperatureC { get; set; }
    public double FeelsLikeC { get; set; }
    public double TempMinC { get; set; }
    public double TempMaxC { get; set; }
    public int HumidityPercent { get; set; }
    public int PressureHPa { get; set; }
    public double WindSpeedMs { get; set; }

    public required string WeatherMain { get; set; }
    public required string WeatherDescription { get; set; }
    public required string WeatherIcon { get; set; }
}
