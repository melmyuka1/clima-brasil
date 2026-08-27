using WeatherDashboard.Domain.Catalog;
using Xunit;

namespace WeatherDashboard.Tests.Domain;

public class CuritibaMetroRegionTests
{
    [Fact]
    public void All_ContainsExactlyTwentyNineMunicipalities()
    {
        Assert.Equal(29, CuritibaMetroRegion.All.Count);
    }

    [Fact]
    public void All_HasUniqueIds()
    {
        Assert.Equal(CuritibaMetroRegion.All.Count, CuritibaMetroRegion.All.Select(c => c.Id).Distinct().Count());
    }

    [Fact]
    public void All_IncludesCuritibaSharedWithCapitalsCatalog()
    {
        var curitiba = CuritibaMetroRegion.FindById("curitiba");

        Assert.NotNull(curitiba);
        Assert.Same(BrazilianCapitals.FindById("curitiba"), curitiba);
    }

    [Fact]
    public void BorderCities_ContainsExactlyTheNineMunicipalitiesThatBorderCuritiba()
    {
        var expectedIds = new[]
        {
            "curitiba", "colombo", "pinhais", "sao-jose-dos-pinhais", "araucaria",
            "campo-largo", "fazenda-rio-grande", "quatro-barras", "piraquara",
        };

        Assert.Equal(expectedIds.Length, CuritibaMetroRegion.BorderCities.Count);
        Assert.Equal(expectedIds.ToHashSet(), CuritibaMetroRegion.BorderCities.Select(c => c.Id).ToHashSet());
    }

    [Fact]
    public void FindById_IsCaseInsensitive()
    {
        var city = CuritibaMetroRegion.FindById("SAO-JOSE-DOS-PINHAIS");

        Assert.NotNull(city);
        Assert.Equal("São José dos Pinhais", city!.City);
    }
}
