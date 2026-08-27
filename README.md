# Clima Brasil

Dashboard web com o histórico climático da Região Metropolitana de Curitiba e
das capitais dos estados brasileiros, construído em **.NET 9 / C#** como duas
aplicações separadas: uma **API** (ASP.NET Core Web API + Swagger) que coleta
os dados e os expõe por HTTP, e um **site** (ASP.NET Core MVC) que é a
interface consumida pelo navegador. Os dados são coletados periodicamente da
[OpenWeatherMap](https://openweathermap.org/current) e persistidos em banco
de dados (EF Core InMemory) para consulta histórica.

## Funcionalidades

- Cidade padrão e tira de destaque com os municípios que fazem fronteira com
  Curitiba (Colombo, Pinhais, São José dos Pinhais, Araucária, Campo Largo,
  Fazenda Rio Grande, Quatro Barras e Piraquara); botão "Ver capitais" alterna
  a tira de destaque para as capitais estaduais.
- Seletor de cidade com os 29 municípios da Região Metropolitana de Curitiba
  e as 27 capitais estaduais (26 estados + Distrito Federal).
- API própria com Swagger (`/swagger`), coletando em background a cada 15
  minutos as 55 cidades rastreadas.
- Filtro por data inicial/final sobre o histórico coletado.
- Dois gráficos: temperatura (mín/média/máx) por dia, e umidade × vento
  médios por dia.
- Cartões com as estatísticas do dia atual (máxima, mínima, média,
  umidade média, vento médio, número de leituras).
- Cena de fundo animada conforme a condição climática atual da cidade
  selecionada: sol, nuvens, chuva, tempestade (com relâmpago) ou céu
  estrelado à noite.
- Layout responsivo (smartphone, tablet, desktop), CSS e HTML5 escritos à
  mão, sem framework de UI pronto.

## Como rodar

A API e o site rodam como dois processos separados — a API precisa estar no
ar para o site mostrar dados.

```bash
dotnet restore
dotnet user-secrets set "OpenWeatherMap:ApiKey" "SUA_CHAVE_AQUI" --project src/WeatherDashboard.Api

# terminal 1
dotnet run --project src/WeatherDashboard.Api
# terminal 2
dotnet run --project src/WeatherDashboard.Web
```

Abra `http://localhost:5170` (site) e `http://localhost:5282/swagger` (API).
Passo a passo completo, configuração da chave de API e deploy:
[docs/wiki/Instalacao-e-Deploy.md](docs/wiki/Instalacao-e-Deploy.md).

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
  WeatherDashboard.Api/             Web API + Swagger — dono dos dados, coleta a cada 15 min
  WeatherDashboard.Web/             site MVC — consome a API direto do navegador (fetch + CORS)
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

Resumo: `Api` e `Web` são dois executáveis independentes, cada um dependendo
de `Infrastructure` → `Domain` (a `Web` depende só de `Domain`, para os
catálogos estáticos de cidade). O `Domain` não conhece EF Core, HTTP ou
ASP.NET, o que mantém a regra de agregação de estatísticas
(`WeatherStatsCalculator`) trivialmente testável e torna a troca de provedor
de banco de dados (InMemory → relacional, se necessário) uma mudança isolada
em um único ponto de composição, dentro da `Api`.
