# Clima Brasil

Dashboard web com o histórico climático das capitais dos estados brasileiros,
construído em **ASP.NET Core MVC (.NET 9 / C#)**. Os dados são coletados
periodicamente da [OpenWeatherMap](https://openweathermap.org/current) e
persistidos em banco de dados (EF Core InMemory) para consulta histórica.

## Funcionalidades

- Seleção de qualquer uma das 27 capitais (26 estados + Distrito Federal).
- Coleta automática em background a cada 15 minutos, para todas as capitais.
- Filtro por data inicial/final sobre o histórico coletado.
- Dois gráficos: temperatura (mín/média/máx) por dia, e umidade × vento
  médios por dia.
- Cartões com as estatísticas do dia atual (máxima, mínima, média,
  umidade média, vento médio, número de leituras).
- Layout responsivo (smartphone, tablet, desktop), CSS e HTML5 escritos à
  mão, sem framework de UI pronto.

## Como rodar

```bash
dotnet restore
dotnet user-secrets set "OpenWeatherMap:ApiKey" "SUA_CHAVE_AQUI" --project src/WeatherDashboard.Web
dotnet run --project src/WeatherDashboard.Web
```

Abra `http://localhost:5170`. Passo a passo completo, opções de configuração
da chave de API e deploy: [docs/wiki/Instalacao-e-Deploy.md](docs/wiki/Instalacao-e-Deploy.md).

## Testes

```bash
dotnet test
```

## Estrutura do projeto

```
WeatherDashboard.sln
src/
  WeatherDashboard.Domain/          entidades, interfaces, regras de negócio puras
  WeatherDashboard.Infrastructure/  EF Core, cliente OpenWeatherMap, coletor em background
  WeatherDashboard.Web/             controllers MVC, views Razor, CSS/JS
tests/
  WeatherDashboard.Tests/           testes xUnit
docs/
  architecture.md                   diagramas (lógico/físico) e decisões de design
  wiki/                             conteúdo para a Wiki do GitHub
```

## Arquitetura e decisões de design

Descrição completa, diagramas de arquitetura lógica/física e as principais
suposições assumidas na interpretação do exercício estão em
[docs/architecture.md](docs/architecture.md).

Resumo: `Web` depende de `Infrastructure`, que depende de `Domain`; o
`Domain` não conhece EF Core, HTTP ou ASP.NET, o que mantém a regra de
agregação de estatísticas (`WeatherStatsCalculator`) trivialmente testável e
torna a troca de provedor de banco de dados (InMemory → relacional, se
necessário) uma mudança isolada em um único ponto de composição.
