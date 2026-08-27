using WeatherDashboard.Domain.Entities;

namespace WeatherDashboard.Domain.Catalog;

/// <summary>
/// União das cidades rastreadas pela aplicação (capitais estaduais + Região Metropolitana de
/// Curitiba), deduplicada por Id — Curitiba aparece nos dois catálogos de origem, mas só uma
/// vez aqui. Usada pelo coletor em background (que não precisa saber de onde cada cidade veio)
/// e pelos controllers, para resolver o parâmetro de cidade vindo de qualquer um dos seletores.
/// </summary>
public static class TrackedCities
{
    public static readonly IReadOnlyList<TrackedCity> All = BrazilianCapitals.All
        .Concat(CuritibaMetroRegion.All)
        .DistinctBy(c => c.Id)
        .ToList();

    public static TrackedCity? FindById(string id) =>
        All.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    /// <summary>Cidade principal exibida por padrão ao abrir o dashboard.</summary>
    public static readonly TrackedCity Default = All.Single(c => c.Id == "curitiba");
}
