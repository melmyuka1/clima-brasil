# Build no contexto da raiz do repositório (o projeto referencia Domain via
# ProjectReference relativo).
#
#   docker build -f src/WeatherDashboard.Web/Dockerfile -t clima-brasil-web .
#
# No Render/Railway/etc.: "Dockerfile Path" = src/WeatherDashboard.Web/Dockerfile,
# "Docker Build Context Directory" = . (raiz do repo).

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY WeatherDashboard.sln .
COPY src/WeatherDashboard.Domain/WeatherDashboard.Domain.csproj src/WeatherDashboard.Domain/
COPY src/WeatherDashboard.Web/WeatherDashboard.Web.csproj src/WeatherDashboard.Web/
RUN dotnet restore src/WeatherDashboard.Web/WeatherDashboard.Web.csproj

COPY src/WeatherDashboard.Domain/ src/WeatherDashboard.Domain/
COPY src/WeatherDashboard.Web/ src/WeatherDashboard.Web/
RUN dotnet publish src/WeatherDashboard.Web/WeatherDashboard.Web.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Provedores como Render/Railway injetam a porta real via $PORT em tempo de
# execução; 8080 é só o padrão para rodar localmente com `docker run`.
ENV ASPNETCORE_ENVIRONMENT=Production

# Desliga o FileSystemWatcher que recarrega o appsettings.json em mudanças.
# Containers de PaaS gratuitos (ex.: Render) têm limite baixo de inotify e o
# watcher padrão derruba o processo no startup com "IOException: user limit
# on inotify instances reached". A config nunca muda em runtime aqui, então
# o reload automático não faz falta.
ENV DOTNET_hostBuilder__reloadConfigOnChange=false

EXPOSE 8080
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet WeatherDashboard.Web.dll"]
