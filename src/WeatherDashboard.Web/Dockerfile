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
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet WeatherDashboard.Web.dll"]
