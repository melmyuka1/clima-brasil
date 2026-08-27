using WeatherDashboard.Domain.Catalog;
using Xunit;

namespace WeatherDashboard.Tests.Domain;

public class BrazilianCapitalsTests
{
    [Fact]
    public void All_ContainsExactlyTwentySevenCapitals()
    {
        // 26 estados + Distrito Federal
        Assert.Equal(27, BrazilianCapitals.All.Count);
    }

    [Fact]
    public void All_HasUniqueIdsAndUfs()
    {
        Assert.Equal(BrazilianCapitals.All.Count, BrazilianCapitals.All.Select(c => c.Id).Distinct().Count());
        Assert.Equal(BrazilianCapitals.All.Count, BrazilianCapitals.All.Select(c => c.Uf).Distinct().Count());
    }

    [Theory]
    [InlineData("sao-paulo")]
    [InlineData("SAO-PAULO")]
    [InlineData("Sao-Paulo")]
    public void FindById_IsCaseInsensitive(string id)
    {
        var capital = BrazilianCapitals.FindById(id);

        Assert.NotNull(capital);
        Assert.Equal("São Paulo", capital!.City);
    }

    [Fact]
    public void FindById_ReturnsNullForUnknownId()
    {
        Assert.Null(BrazilianCapitals.FindById("atlantis"));
    }
}
