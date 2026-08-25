using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WeatherDashboard.Domain.Interfaces;
using WeatherDashboard.Infrastructure.BackgroundServices;
using WeatherDashboard.Infrastructure.Data;
using WeatherDashboard.Infrastructure.ExternalServices;

namespace WeatherDashboard.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Registra banco de dados (InMemory), cliente HTTP da OpenWeatherMap, repositório e o
    /// coletor em background. Ponto único de composição da camada de infraestrutura — trocar
    /// o provedor de banco de dados (ex.: SQL Server/SQLite em produção) exige alterar apenas
    /// a chamada UseInMemoryDatabase abaixo.
    /// </summary>
    public static IServiceCollection AddWeatherInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<WeatherDbContext>(options =>
            options.UseInMemoryDatabase("WeatherDashboardDb"));

        services.Configure<OpenWeatherMapOptions>(configuration.GetSection(OpenWeatherMapOptions.SectionName));
        services.Configure<WeatherCollectorOptions>(configuration.GetSection(WeatherCollectorOptions.SectionName));

        services.AddHttpClient<IWeatherApiClient, OpenWeatherMapClient>();

        services.AddScoped<IWeatherRecordRepository, EfWeatherRecordRepository>();

        services.AddHostedService<WeatherCollectorHostedService>();

        return services;
    }
}
