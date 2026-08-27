using WeatherDashboard.Domain.Entities;

namespace WeatherDashboard.Domain.Catalog;

/// <summary>
/// Catálogo estático com as 26 capitais estaduais + Distrito Federal, usado para
/// popular o seletor de cidades e orientar a coleta periódica de dados.
/// Coordenadas aproximadas do centro de cada capital.
/// </summary>
public static class BrazilianCapitals
{
    public static readonly IReadOnlyList<TrackedCity> All = new List<TrackedCity>
    {
        new() { Id = "rio-branco", City = "Rio Branco", Uf = "AC", State = "Acre", Latitude = -9.97499, Longitude = -67.8243 },
        new() { Id = "maceio", City = "Maceió", Uf = "AL", State = "Alagoas", Latitude = -9.66599, Longitude = -35.735 },
        new() { Id = "macapa", City = "Macapá", Uf = "AP", State = "Amapá", Latitude = 0.034934, Longitude = -51.0694 },
        new() { Id = "manaus", City = "Manaus", Uf = "AM", State = "Amazonas", Latitude = -3.10194, Longitude = -60.025 },
        new() { Id = "salvador", City = "Salvador", Uf = "BA", State = "Bahia", Latitude = -12.9714, Longitude = -38.5014 },
        new() { Id = "fortaleza", City = "Fortaleza", Uf = "CE", State = "Ceará", Latitude = -3.71722, Longitude = -38.5433 },
        new() { Id = "brasilia", City = "Brasília", Uf = "DF", State = "Distrito Federal", Latitude = -15.7797, Longitude = -47.9297 },
        new() { Id = "vitoria", City = "Vitória", Uf = "ES", State = "Espírito Santo", Latitude = -20.3155, Longitude = -40.3128 },
        new() { Id = "goiania", City = "Goiânia", Uf = "GO", State = "Goiás", Latitude = -16.6799, Longitude = -49.255 },
        new() { Id = "sao-luis", City = "São Luís", Uf = "MA", State = "Maranhão", Latitude = -2.53874, Longitude = -44.2825 },
        new() { Id = "cuiaba", City = "Cuiabá", Uf = "MT", State = "Mato Grosso", Latitude = -15.601, Longitude = -56.0974 },
        new() { Id = "campo-grande", City = "Campo Grande", Uf = "MS", State = "Mato Grosso do Sul", Latitude = -20.4697, Longitude = -54.6201 },
        new() { Id = "belo-horizonte", City = "Belo Horizonte", Uf = "MG", State = "Minas Gerais", Latitude = -19.9167, Longitude = -43.9345 },
        new() { Id = "belem", City = "Belém", Uf = "PA", State = "Pará", Latitude = -1.45502, Longitude = -48.5024 },
        new() { Id = "joao-pessoa", City = "João Pessoa", Uf = "PB", State = "Paraíba", Latitude = -7.11509, Longitude = -34.8641 },
        new() { Id = "curitiba", City = "Curitiba", Uf = "PR", State = "Paraná", Latitude = -25.4284, Longitude = -49.2733 },
        new() { Id = "recife", City = "Recife", Uf = "PE", State = "Pernambuco", Latitude = -8.04756, Longitude = -34.877 },
        new() { Id = "teresina", City = "Teresina", Uf = "PI", State = "Piauí", Latitude = -5.08921, Longitude = -42.8016 },
        new() { Id = "rio-de-janeiro", City = "Rio de Janeiro", Uf = "RJ", State = "Rio de Janeiro", Latitude = -22.9068, Longitude = -43.1729 },
        new() { Id = "natal", City = "Natal", Uf = "RN", State = "Rio Grande do Norte", Latitude = -5.79448, Longitude = -35.211 },
        new() { Id = "porto-alegre", City = "Porto Alegre", Uf = "RS", State = "Rio Grande do Sul", Latitude = -30.0346, Longitude = -51.2177 },
        new() { Id = "porto-velho", City = "Porto Velho", Uf = "RO", State = "Rondônia", Latitude = -8.76183, Longitude = -63.9039 },
        new() { Id = "boa-vista", City = "Boa Vista", Uf = "RR", State = "Roraima", Latitude = 2.82384, Longitude = -60.6753 },
        new() { Id = "florianopolis", City = "Florianópolis", Uf = "SC", State = "Santa Catarina", Latitude = -27.5954, Longitude = -48.548 },
        new() { Id = "sao-paulo", City = "São Paulo", Uf = "SP", State = "São Paulo", Latitude = -23.5505, Longitude = -46.6333 },
        new() { Id = "aracaju", City = "Aracaju", Uf = "SE", State = "Sergipe", Latitude = -10.9472, Longitude = -37.0731 },
        new() { Id = "palmas", City = "Palmas", Uf = "TO", State = "Tocantins", Latitude = -10.1689, Longitude = -48.3317 },
    };

    public static TrackedCity? FindById(string id) =>
        All.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
}
