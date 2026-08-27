# Build no contexto da raiz do repositório (o projeto referencia Domain e
# Infrastructure via ProjectReference relativo, então o contexto precisa
# enxergar a solução inteira, não só esta pasta).
#
#   docker build -f src/WeatherDashboard.Api/Dockerfile -t clima-brasil-api .
#
# No Render/Railway/etc.: "Dockerfile Path" = src/WeatherDashboard.Api/Dockerfile,
# "Docker Build Context Directory" = . (raiz do repo).

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY WeatherDashboard.sln .
COPY src/WeatherDashboard.Domain/WeatherDashboard.Domain.csproj src/WeatherDashboard.Domain/
COPY src/WeatherDashboard.Infrastructure/WeatherDashboard.Infrastructure.csproj src/WeatherDashboard.Infrastructure/
COPY src/WeatherDashboard.Api/WeatherDashboard.Api.csproj src/WeatherDashboard.Api/
RUN dotnet restore src/WeatherDashboard.Api/WeatherDashboard.Api.csproj

COPY src/WeatherDashboard.Domain/ src/WeatherDashboard.Domain/
COPY src/WeatherDashboard.Infrastructure/ src/WeatherDashboard.Infrastructure/
COPY src/WeatherDashboard.Api/ src/WeatherDashboard.Api/
RUN dotnet publish src/WeatherDashboard.Api/WeatherDashboard.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Provedores como Render/Railway injetam a porta real via $PORT em tempo de
# execução; 8080 é só o padrão para rodar localmente com `docker run`.
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet WeatherDashboard.Api.dll"]
