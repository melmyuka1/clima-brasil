using System.Reflection;
using WeatherDashboard.Api.Json;
using WeatherDashboard.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

const string WebClientCorsPolicy = "WebClient";

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter()));

builder.Services.AddWeatherInfrastructure(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Clima Brasil API",
        Version = "v1",
        Description = "Histórico climático da Região Metropolitana de Curitiba e das capitais " +
                      "estaduais brasileiras, coletado a cada 15 minutos da OpenWeatherMap.",
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// A UI (WeatherDashboard.Web) roda em outra origem/porta e chama esta API diretamente do
// navegador — precisa de CORS explícito. As origens permitidas vêm de configuração para que o
// deploy possa restringir ao domínio real do site em produção.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5170", "https://localhost:7034"];

builder.Services.AddCors(options =>
{
    options.AddPolicy(WebClientCorsPolicy, policy =>
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .WithMethods("GET"));
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Clima Brasil API v1");
    options.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseCors(WebClientCorsPolicy);
app.UseAuthorization();
app.MapControllers();

app.Run();

/// <summary>Classe parcial exposta para permitir testes de integração com WebApplicationFactory.</summary>
public partial class Program;
