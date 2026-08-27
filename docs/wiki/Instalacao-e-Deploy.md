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

A chave é configurada no projeto **`WeatherDashboard.Api`** — é ele quem
coleta os dados. O `WeatherDashboard.Web` não usa nem precisa da chave.

### Opção A — User Secrets (recomendado para desenvolvimento local)

```bash
cd src/WeatherDashboard.Api
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

Em `src/WeatherDashboard.Api/appsettings.Development.json`:

```json
{
  "OpenWeatherMap": { "ApiKey": "SUA_CHAVE_AQUI" }
}
```

> Sem chave configurada, a API sobe normalmente — o coletor apenas
> registra um aviso no log e pula a coleta, e o dashboard mostra um estado
> vazio até a chave ser configurada.

## 3. Restaurar, compilar e rodar

Na raiz do repositório (onde está `WeatherDashboard.sln`):

```bash
dotnet restore
dotnet build
```

São **dois processos separados** — a API precisa estar rodando para o site
mostrar dados. Abra dois terminais:

```bash
# terminal 1 — API (coleta os dados e os expõe por HTTP)
dotnet run --project src/WeatherDashboard.Api
```

```bash
# terminal 2 — site (a interface que o usuário abre no navegador)
dotnet run --project src/WeatherDashboard.Web
```

- Site: `http://localhost:5170`
- API / Swagger: `http://localhost:5282/swagger`

Abra o site nessa URL — o dashboard já tenta uma coleta imediatamente ao
iniciar a API, então em poucos segundos (com a chave configurada) os dados
da cidade padrão (Curitiba) aparecem.

A cada 15 minutos, um `BackgroundService` (`WeatherCollectorHostedService`,
rodando dentro do processo da `Api`) consulta a OpenWeatherMap para as 55
cidades rastreadas (27 capitais + 29 municípios da Região Metropolitana de
Curitiba) e grava cada leitura no banco em memória; o front-end reconsulta a
API a cada 5 minutos, então novas coletas aparecem no dashboard sem precisar
recarregar a página.

Se o site e a API rodarem em portas diferentes das padrão, ajuste
`WeatherApi:BaseUrlHttp` / `BaseUrlHttps` em
`src/WeatherDashboard.Web/appsettings.json` (URLs que o navegador usa para
chamar a API — o JavaScript escolhe a que casa com o protocolo da própria
página, pra não ser bloqueado como "conteúdo misto") e `Cors:AllowedOrigins`
em `src/WeatherDashboard.Api/appsettings.json` (origens que a API aceita).

> **Rodando via IDE (Visual Studio, Rider) em vez de `dotnet run`?** Garanta
> que a `Api` suba com um perfil que escute nas duas portas (http **e**
> https) — o perfil padrão do projeto já faz isso. Se só a porta http estiver
> escutando e o site abrir em https (ou vice-versa), a chamada à API falha
> com "NetworkError" (Firefox) ou "Failed to fetch" (Chrome) por conteúdo
> misto — não é um bug de dados, é só as duas portas não combinando.

## 4. Rodar os testes automatizados

```bash
dotnet test
```

## 5. Publicar / gerar build de produção

```bash
dotnet publish src/WeatherDashboard.Api -c Release -o ./publish-api
dotnet publish src/WeatherDashboard.Web -c Release -o ./publish-web
```

Isso gera os artefatos prontos para deploy em `./publish-api` e
`./publish-web` — dois processos, cada um com sua própria porta/domínio.
Configure a chave da API no ambiente de destino da `Api` (variável de
ambiente `OpenWeatherMap__ApiKey`, ou um `appsettings.Production.json` fora
do controle de versão), e aponte `WeatherApi:BaseUrl` do `Web` para a URL
pública da `Api` em produção — junto com `Cors:AllowedOrigins` na `Api`
apontando de volta para a URL pública do `Web`.

### Opções de deploy

- **IIS / Windows Server**: publicar os dois projetos com `dotnet publish` e
  hospedar cada um como um site/aplicativo próprio via módulo ASP.NET Core
  (`ASPNETCORE_ENVIRONMENT=Production`).
- **Container Docker**: uma imagem por projeto, ambas baseadas em
  `mcr.microsoft.com/dotnet/aspnet:9.0`, cada uma expondo a porta configurada
  em `ASPNETCORE_URLS`.
- **Azure App Service / qualquer PaaS com suporte a .NET 9**: dois App
  Services (um para a `Api`, um para o `Web`), publicando cada um via
  `dotnet publish` + deploy do pacote e configurando as variáveis acima.

### Hospedagem gratuita (fora o GitHub)

**GitHub Pages não serve para este projeto** — ele só hospeda site estático
(HTML/CSS/JS puro, sem servidor por trás). Esta aplicação precisa rodar um
processo .NET (a `Api`, com o coletor em background e o banco em memória), o
que o GitHub Pages não suporta em nenhuma hipótese. Duas alternativas
gratuitas que rodam um backend .NET de verdade:

- **[Azure App Service](https://azure.microsoft.com/free/) — tier gratuito F1**:
  opção mais "nativa" pra .NET (mesmo fabricante do framework). Cria-se um
  App Service por projeto (`clima-brasil-api`, `clima-brasil-web`), plano
  de preço **F1 (Free)**. Deploy direto do Visual Studio ("Publish" → Azure)
  ou via `dotnet publish` + `az webapp deploy`. Configura-se a chave da
  OpenWeatherMap e as URLs (`WeatherApi:BaseUrlHttps`, `Cors:AllowedOrigins`)
  em "Configuration → Application settings" no portal, sem precisar tocar
  no código. Limitação do F1: a app "dorme" após um tempo sem uso e demora
  alguns segundos para acordar na próxima visita — normal em tier gratuito,
  não é bug.
- **[Render](https://render.com/) — Free Web Service**: alternativa fora do
  ecossistema Microsoft, baseada em Docker (adicionar um `Dockerfile` simples
  por projeto, usando `mcr.microsoft.com/dotnet/aspnet:9.0` como imagem
  base). Também dorme após inatividade no tier gratuito. Boa opção se você já
  usa Render pra outros projetos ou prefere não criar conta Azure.

Em ambos os casos, a chave da OpenWeatherMap **nunca** vai no código — sempre
como variável de ambiente/"application setting" configurada no painel do
provedor, do mesmo jeito que o user-secrets faz localmente.

> Como o banco é EF Core InMemory, os dados **não sobrevivem** a um reinício
> do processo da `Api` — isso é intencional para o escopo deste exercício.
> Para um ambiente que precise reter histórico entre reinícios, troque o
> provedor em
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
