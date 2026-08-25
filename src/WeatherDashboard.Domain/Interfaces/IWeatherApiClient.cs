using WeatherDashboard.Domain.Entities;

namespace WeatherDashboard.Domain.Interfaces;

/// <summary>
/// Abstração sobre o provedor externo de clima (OpenWeatherMap). Mantém o restante
/// da aplicação isolado do formato de resposta e da biblioteca HTTP usada.
/// </summary>
public interface IWeatherApiClient
{
    /// <summary>
    /// Busca a condição climática atual para uma coordenada. Retorna null quando a chamada
    /// falha (erro de rede, chave inválida, rate limit) — o chamador decide como lidar com isso.
    /// </summary>
    Task<WeatherRecord?> GetCurrentWeatherAsync(BrazilianCapital capital, CancellationToken cancellationToken = default);
}
