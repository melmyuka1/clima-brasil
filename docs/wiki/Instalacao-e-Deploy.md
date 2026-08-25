# Instalação e Deploy

## Pré-requisitos

- [.NET SDK 9.0](https://dotnet.microsoft.com/download) ou superior.
- Uma chave de API gratuita da OpenWeatherMap: crie uma conta em
  <https://openweathermap.org/api> e gere uma "API key" (o plano gratuito
  "Current Weather Data" atende ao projeto).
- Git.

## 1. Clonar o repositório

```bash
git clone <URL-do-seu-repositório>
cd teste
```

## 2. Configurar a chave da API

A chave **não** deve ser commitada no `appsettings.json` (ele fica com
`OpenWeatherMap:ApiKey` vazio de propósito). Escolha uma das opções abaixo.

### Opção A — User Secrets (recomendado para desenvolvimento local)

```bash
cd src/WeatherDashboard.Web
dotnet user-secrets init
dotnet user-secrets set "OpenWeatherMap:ApiKey" "SUA_CHAVE_AQUI"
```

### Opção B — Variável de ambiente

```bash
# PowerShell
$env:OpenWeatherMap__ApiKey = "SUA_CHAVE_AQUI"

# bash
export OpenWeatherMap__ApiKey="SUA_CHAVE_AQUI"
```

### Opção C — appsettings.Development.json local (não commitar)

```json
{
  "OpenWeatherMap": { "ApiKey": "SUA_CHAVE_AQUI" }
}
```

> Sem chave configurada, a aplicação sobe normalmente — o coletor apenas
> registra um aviso no log e pula a coleta, e o dashboard mostra um estado
> vazio até a chave ser configurada.

## 3. Restaurar, compilar e rodar

Na raiz do repositório (onde está `WeatherDashboard.sln`):

```bash
dotnet restore
dotnet build
dotnet run --project src/WeatherDashboard.Web
```

A aplicação sobe em `http://localhost:5170` (e `https://localhost:7034`, se
o perfil `https` for usado). Abra o navegador nessa URL — o dashboard já
tenta uma coleta imediatamente ao iniciar, então em poucos segundos (com a
chave configurada) os dados da capital padrão (São Paulo) aparecem.

A cada 15 minutos, um `BackgroundService` (`WeatherCollectorHostedService`)
consulta a OpenWeatherMap para as 27 capitais e grava cada leitura no banco
em memória; o front-end reconsulta o servidor a cada 5 minutos, então novas
coletas aparecem no dashboard sem precisar recarregar a página.

## 4. Rodar os testes automatizados

```bash
dotnet test
```

## 5. Publicar / gerar build de produção

```bash
dotnet publish src/WeatherDashboard.Web -c Release -o ./publish
```

Isso gera os artefatos prontos para deploy em `./publish`. Configure a chave
da API no ambiente de destino (variável de ambiente `OpenWeatherMap__ApiKey`,
ou um `appsettings.Production.json` fora do controle de versão).

### Opções de deploy

- **IIS / Windows Server**: publicar com `dotnet publish` e hospedar via
  módulo ASP.NET Core (`ASPNETCORE_ENVIRONMENT=Production`).
- **Container Docker**: usar a imagem `mcr.microsoft.com/dotnet/aspnet:9.0`
  como base, copiar a pasta `publish` e expor a porta configurada em
  `ASPNETCORE_URLS`.
- **Azure App Service / qualquer PaaS com suporte a .NET 9**: publicar
  diretamente via `dotnet publish` + deploy do pacote, configurando a chave
  da API como variável de ambiente/app setting.

> Como o banco é EF Core InMemory, os dados **não sobrevivem** a um reinício
> do processo — isso é intencional para o escopo deste exercício. Para um
> ambiente que precise reter histórico entre reinícios, troque o provedor em
> [`InfrastructureServiceCollectionExtensions.cs`](../../src/WeatherDashboard.Infrastructure/InfrastructureServiceCollectionExtensions.cs)
> (ex.: `UseSqlite(...)` ou `UseSqlServer(...)`) — nenhuma outra camada
> precisa mudar, pois todo acesso a dados passa por `IWeatherRecordRepository`.

## Publicando esta pasta como Wiki do GitHub

O GitHub Wiki é, por baixo dos panos, outro repositório Git
(`<repo>.wiki.git`). Para publicar o conteúdo de `docs/wiki/` lá:

```bash
git clone https://github.com/<usuario>/<repo>.wiki.git
cp docs/wiki/*.md <repo>.wiki/
cd <repo>.wiki
git add .
git commit -m "Publica wiki do projeto"
git push
```

(A primeira página, `Home.md`, vira automaticamente a página inicial da
Wiki.)
