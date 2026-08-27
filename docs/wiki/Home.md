# Clima Brasil — Wiki

Dashboard climático histórico da Região Metropolitana de Curitiba e das
capitais dos estados brasileiros, feito em .NET 9 como duas aplicações: uma
API (ASP.NET Core Web API + Swagger) que coleta os dados, e um site
(ASP.NET Core MVC) que os exibe. Os dados vêm da
[OpenWeatherMap](https://openweathermap.org/current).

> Estes arquivos ficam em `docs/wiki/` no repositório. Para publicá-los como a
> Wiki do GitHub, veja a seção "Publicando esta pasta como Wiki do GitHub" em
> [Instalacao-e-Deploy.md](Instalacao-e-Deploy.md).

## Páginas

- [Instalação e Deploy](Instalacao-e-Deploy.md) — pré-requisitos, como rodar
  os dois processos localmente, como configurar a chave da API, como rodar
  os testes e opções de deploy.
- [Arquitetura](../architecture.md) — diagramas lógico e físico, decisões de
  design e suposições.
- [Como Testar](Como-Testar.md) — o que os 39 testes automatizados cobrem, e
  um roteiro manual pra validar dashboard, API e responsividade.

## Resumo do projeto

- **Backend**: dois executáveis .NET 9 / C# — `WeatherDashboard.Api`
  (ASP.NET Core Web API, com Swagger UI em `/swagger`) e
  `WeatherDashboard.Web` (ASP.NET Core MVC).
- **Persistência**: Entity Framework Core, provedor **InMemory** (recomendado
  pelo enunciado para testes/avaliação), dono pela `Api`.
- **Fonte de dados**: OpenWeatherMap — "Current Weather Data" endpoint,
  consultado a cada 15 minutos para 55 cidades (27 capitais brasileiras + os
  29 municípios da Região Metropolitana de Curitiba) por um `BackgroundService`
  rodando dentro da `Api`.
- **Frontend**: Razor Views + HTML5/CSS3 próprios (responsivo, sem
  frameworks de UI prontos), Chart.js para os gráficos e uma cena de fundo
  animada (sol/nuvens/chuva/tempestade/céu estrelado) conforme a condição
  climática atual — tudo buscado pelo navegador direto na `Api` via fetch/CORS.
- **Testes**: xUnit + Moq, cobrindo regras de agregação, repositório,
  cliente HTTP e os controllers de ambos os projetos.
