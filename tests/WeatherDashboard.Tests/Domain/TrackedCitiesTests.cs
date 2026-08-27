using WeatherDashboard.Domain.Catalog;
using Xunit;

namespace WeatherDashboard.Tests.Domain;

public class TrackedCitiesTests
{
    [Fact]
    public void All_IsTheUnionOfCapitalsAndMetroRegionWithoutDuplicatingCuritiba()
    {
        // 27 capitais + 29 municípios da RMC - 1 (Curitiba conta nos dois catálogos de origem)
        Assert.Equal(27 + 29 - 1, TrackedCities.All.Count);
        Assert.Equal(TrackedCities.All.Count, TrackedCities.All.Select(c => c.Id).Distinct().Count());
    }

    [Fact]
    public void FindById_ResolvesCitiesFromEitherCatalog()
    {
        Assert.NotNull(TrackedCities.FindById("sao-paulo")); // capital
        Assert.NotNull(TrackedCities.FindById("colombo")); // município da RMC
    }

    [Fact]
    public void Default_IsCuritiba()
    {
        Assert.Equal("curitiba", TrackedCities.Default.Id);
    }
}
