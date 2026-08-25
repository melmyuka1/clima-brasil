using System.Net;

namespace WeatherDashboard.Tests.Infrastructure;

/// <summary>HttpMessageHandler de teste que devolve uma resposta fixa, sem tocar a rede.</summary>
internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string? _jsonBody;
    private readonly Exception? _exceptionToThrow;

    public FakeHttpMessageHandler(HttpStatusCode statusCode, string? jsonBody)
    {
        _statusCode = statusCode;
        _jsonBody = jsonBody;
    }

    public FakeHttpMessageHandler(Exception exceptionToThrow)
    {
        _exceptionToThrow = exceptionToThrow;
        _statusCode = HttpStatusCode.InternalServerError;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_exceptionToThrow is not null)
        {
            throw _exceptionToThrow;
        }

        var response = new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_jsonBody ?? string.Empty),
        };
        return Task.FromResult(response);
    }
}
