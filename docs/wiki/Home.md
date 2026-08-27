# Clima Brasil — Wiki

Dashboard climático histórico da Região Metropolitana de Curitiba e das
capitais dos estados brasileiros, feito em ASP.NET Core MVC (.NET 9) com dados
coletados periodicamente da [OpenWeatherMap](https://openweathermap.org/current).

> Estes arquivos ficam em `docs/wiki/` no repositório. Para publicá-los como a
> Wiki do GitHub, veja a seção "Publicando esta pasta como Wiki do GitHub" em
> [Instalacao-e-Deploy.md](Instalacao-e-Deploy.md).

## Páginas

- [Instalação e Deploy](Instalacao-e-Deploy.md) — pré-requisitos, como rodar
  localmente, como configurar a chave da API, como rodar os testes e opções
  de deploy.
- [Arquitetura](../architecture.md) — diagramas lógico e físico, decisões de
  design e suposições.

## Resumo do projeto

- **Backend**: ASP.NET Core MVC (.NET 9), C#.
- **Persistência**: Entity Framework Core, provedor **InMemory** (recomendado
  pelo enunciado para testes/avaliação).
- **Fonte de dados**: OpenWeatherMap — "Current Weather Data" endpoint,
  consultado a cada 15 minutos para 55 cidades (27 capitais brasileiras + os
  29 municípios da Região Metropolitana de Curitiba) por um `BackgroundService`.
- **Frontend**: Razor Views + HTML5/CSS3 próprios (responsivo, sem
  frameworks de UI prontos) e Chart.js para os gráficos.
- **Testes**: xUnit + Moq, cobrindo regras de agregação, repositório,
  cliente HTTP e controller.
