# Clima Brasil — Wiki

Dashboard climático histórico das capitais brasileiras, feito em ASP.NET Core
MVC (.NET 9) com dados coletados periodicamente da [OpenWeatherMap](https://openweathermap.org/current).

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
  consultado a cada 15 minutos para as 27 capitais brasileiras (26 estados +
  Distrito Federal) por um `BackgroundService`.
- **Frontend**: Razor Views + HTML5/CSS3 próprios (responsivo, sem
  frameworks de UI prontos) e Chart.js para os gráficos.
- **Testes**: xUnit + Moq, cobrindo regras de agregação, repositório,
  cliente HTTP e controller.
