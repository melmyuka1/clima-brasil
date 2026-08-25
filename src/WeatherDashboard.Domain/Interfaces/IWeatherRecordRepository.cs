using WeatherDashboard.Domain.Entities;

namespace WeatherDashboard.Domain.Interfaces;

public interface IWeatherRecordRepository
{
    Task AddAsync(WeatherRecord record, CancellationToken cancellationToken = default);

    /// <summary>Retorna os registros de uma cidade cuja data de observação (UTC) está no intervalo [start, end], inclusive.</summary>
    Task<IReadOnlyList<WeatherRecord>> GetByCityAndRangeAsync(
        string cityId,
        DateOnly start,
        DateOnly end,
        CancellationToken cancellationToken = default);
}
