using WeatherDashboard.Domain.Entities;

namespace WeatherDashboard.Domain.Catalog;

/// <summary>
/// Catálogo estático dos municípios da Região Metropolitana de Curitiba (RMC).
/// É o conjunto de cidades priorizado pelo dashboard: aparece por padrão ao abrir a
/// aplicação, em vez das capitais estaduais (que ficam disponíveis via alternância).
/// Coordenadas aproximadas do centro de cada município.
/// </summary>
public static class CuritibaMetroRegion
{
    public static readonly IReadOnlyList<TrackedCity> All = new List<TrackedCity>
    {
        BrazilianCapitals.All.Single(c => c.Id == "curitiba"),
        new() { Id = "adrianopolis", City = "Adrianópolis", Uf = "PR", State = "Paraná", Latitude = -24.6669, Longitude = -48.9903 },
        new() { Id = "agudos-do-sul", City = "Agudos do Sul", Uf = "PR", State = "Paraná", Latitude = -25.9836, Longitude = -49.5975 },
        new() { Id = "almirante-tamandare", City = "Almirante Tamandaré", Uf = "PR", State = "Paraná", Latitude = -25.3247, Longitude = -49.3103 },
        new() { Id = "araucaria", City = "Araucária", Uf = "PR", State = "Paraná", Latitude = -25.5925, Longitude = -49.4104 },
        new() { Id = "balsa-nova", City = "Balsa Nova", Uf = "PR", State = "Paraná", Latitude = -25.5772, Longitude = -49.6289 },
        new() { Id = "bocaiuva-do-sul", City = "Bocaiúva do Sul", Uf = "PR", State = "Paraná", Latitude = -25.2072, Longitude = -49.1156 },
        new() { Id = "campina-grande-do-sul", City = "Campina Grande do Sul", Uf = "PR", State = "Paraná", Latitude = -25.3058, Longitude = -49.0553 },
        new() { Id = "campo-do-tenente", City = "Campo do Tenente", Uf = "PR", State = "Paraná", Latitude = -25.9853, Longitude = -49.6825 },
        new() { Id = "campo-largo", City = "Campo Largo", Uf = "PR", State = "Paraná", Latitude = -25.4592, Longitude = -49.5322 },
        new() { Id = "campo-magro", City = "Campo Magro", Uf = "PR", State = "Paraná", Latitude = -25.3319, Longitude = -49.4239 },
        new() { Id = "cerro-azul", City = "Cerro Azul", Uf = "PR", State = "Paraná", Latitude = -24.8181, Longitude = -49.2569 },
        new() { Id = "colombo", City = "Colombo", Uf = "PR", State = "Paraná", Latitude = -25.2917, Longitude = -49.2244 },
        new() { Id = "contenda", City = "Contenda", Uf = "PR", State = "Paraná", Latitude = -25.6733, Longitude = -49.5314 },
        new() { Id = "doutor-ulysses", City = "Doutor Ulysses", Uf = "PR", State = "Paraná", Latitude = -24.5678, Longitude = -49.3103 },
        new() { Id = "fazenda-rio-grande", City = "Fazenda Rio Grande", Uf = "PR", State = "Paraná", Latitude = -25.6608, Longitude = -49.3072 },
        new() { Id = "itaperucu", City = "Itaperuçu", Uf = "PR", State = "Paraná", Latitude = -25.2114, Longitude = -49.3358 },
        new() { Id = "lapa", City = "Lapa", Uf = "PR", State = "Paraná", Latitude = -25.7628, Longitude = -49.7147 },
        new() { Id = "mandirituba", City = "Mandirituba", Uf = "PR", State = "Paraná", Latitude = -25.7756, Longitude = -49.3272 },
        new() { Id = "pien", City = "Piên", Uf = "PR", State = "Paraná", Latitude = -25.9628, Longitude = -49.4550 },
        new() { Id = "pinhais", City = "Pinhais", Uf = "PR", State = "Paraná", Latitude = -25.4419, Longitude = -49.1953 },
        new() { Id = "piraquara", City = "Piraquara", Uf = "PR", State = "Paraná", Latitude = -25.4419, Longitude = -49.0631 },
        new() { Id = "quatro-barras", City = "Quatro Barras", Uf = "PR", State = "Paraná", Latitude = -25.3667, Longitude = -49.0742 },
        new() { Id = "rio-branco-do-sul", City = "Rio Branco do Sul", Uf = "PR", State = "Paraná", Latitude = -25.1889, Longitude = -49.3131 },
        new() { Id = "rio-negro", City = "Rio Negro", Uf = "PR", State = "Paraná", Latitude = -26.1050, Longitude = -49.7994 },
        new() { Id = "quitandinha", City = "Quitandinha", Uf = "PR", State = "Paraná", Latitude = -25.8564, Longitude = -49.5081 },
        new() { Id = "sao-jose-dos-pinhais", City = "São José dos Pinhais", Uf = "PR", State = "Paraná", Latitude = -25.5347, Longitude = -49.2058 },
        new() { Id = "tijucas-do-sul", City = "Tijucas do Sul", Uf = "PR", State = "Paraná", Latitude = -25.9319, Longitude = -48.9328 },
        new() { Id = "tunas-do-parana", City = "Tunas do Paraná", Uf = "PR", State = "Paraná", Latitude = -24.9825, Longitude = -49.1075 },
    };

    /// <summary>
    /// Municípios que fazem fronteira direta com Curitiba — o subconjunto exibido na tira
    /// de destaque por padrão (os demais 20 continuam selecionáveis pelo seletor de cidade).
    /// </summary>
    private static readonly string[] BorderCityIds =
    {
        "curitiba", "colombo", "pinhais", "sao-jose-dos-pinhais", "araucaria",
        "campo-largo", "fazenda-rio-grande", "quatro-barras", "piraquara",
    };

    public static readonly IReadOnlyList<TrackedCity> BorderCities = BorderCityIds
        .Select(id => All.Single(c => c.Id == id))
        .ToList();

    public static TrackedCity? FindById(string id) =>
        All.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
}
