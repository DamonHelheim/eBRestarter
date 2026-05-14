using eBRestarter.Core.Application.Interfaces.RestClient;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models.Api;
using Microsoft.Extensions.Logging;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;

namespace eBRestarter.Infrastructure.Services.RestSharp;

public class RestSharpClientService(ILogger<RestSharpClientService> logger, HttpMessageHandler? httpMessageHandler = null) : IRestClientService
{
    private readonly ILogger<RestSharpClientService> _logger = logger;
    private readonly HttpMessageHandler? _httpMessageHandler = httpMessageHandler;

    public async Task<ApiResponse> ExecuteGetAsync(ApiRequest request)
    {
        // Client erstellen (using sorgt für Dispose)
        using var client = CreateClient(request);

        var restRequest = new RestRequest();

        try
        {
            var response = await client.ExecuteGetAsync(restRequest);
            return MapResponse(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim asynchronen API-Aufruf: {Url}", request.Url);
            return MapExceptionToResponse(ex);
        }
    }

    public ApiResponse ExecuteGet(ApiRequest request)
    {
        using var client = CreateClient(request);

        var restRequest = new RestRequest();

        try
        {
            var response = client.ExecuteGet(restRequest);
            return MapResponse(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim synchronen API-Aufruf: {Url}", request.Url);
            return MapExceptionToResponse(ex);
        }
    }

    private RestClient CreateClient(ApiRequest model)
    {
        var options = new RestClientOptions(model.Url)
        {
            Timeout = TimeSpan.FromSeconds(model.TimeoutSeconds)
        };

        //Dem RestClient den Test-Handler unterschieben
        if (_httpMessageHandler is not null)
        {
            options.ConfigureMessageHandler = _ => _httpMessageHandler;
        }

        if (!string.IsNullOrWhiteSpace(model.Username) && !string.IsNullOrWhiteSpace(model.Password))
        {
            options.Authenticator = new HttpBasicAuthenticator(model.Username, model.Password);
        }

        return new RestClient(options);
    }

    private ApiResponse MapResponse(RestResponse response)
    {
        // 1. Erfolgsfall
        if (response.IsSuccessful)
        {
            var rateCode = CheckRateLimit(response);
            return new ApiResponse
            {
                IsSuccess = true,
                Content = response.Content,
                StatusCode = rateCode
            };
        }
        // Hier ist response.StatusCode in der Regel '0'
        if (response.ResponseStatus == ResponseStatus.TimedOut)
        {
            _logger.LogWarning("API error: Local timeout at {Url}", response.Request.Resource);

            return new ApiResponse
            {
                IsSuccess = false,
                StatusCode = ResponseCode.HTTPTimeout,
                ErrorMessage = "The request has exceeded the time limit"
            };
        }

        // 2. HTTP Fehler Mapping (Status Code -> Dein Enum)
        var errorCode = response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => ResponseCode.HttpRE401,
            HttpStatusCode.TooManyRequests => ResponseCode.HttpRE429,
            HttpStatusCode.InternalServerError => ResponseCode.InternalServerError,
            HttpStatusCode.RequestTimeout => ResponseCode.HTTPTimeout, // Server meldet Timeout
            HttpStatusCode.GatewayTimeout => ResponseCode.HTTPTimeout,
            _ => ResponseCode.NoConnectionToServer // Fallback
        };

        _logger.LogWarning("API Fehler: {Status} - {Msg}", response.StatusCode, response.ErrorMessage);

        return new ApiResponse
        {
            IsSuccess = false,
            StatusCode = errorCode,
            ErrorMessage = response.ErrorMessage ?? response.StatusDescription
        };
    }

    private static ResponseCode CheckRateLimit(RestResponse response)
    {
        // Versuche den Header sicher zu lesen
        var header = response.Headers?.FirstOrDefault(h => h.Name == "X-Ratelimit-Remaining")?.Value?.ToString();

        if (int.TryParse(header, out int remaining) && remaining <= 10)
        {
            return ResponseCode.RequestLimit;
        }

        return ResponseCode.Success;

    }

    private static ApiResponse MapExceptionToResponse(Exception ex)
    {
        // Mapping von .NET Exceptions zu deinem Enum
        var code = ex switch
        {
            TimeoutException => ResponseCode.HTTPTimeout,

            TaskCanceledException => ResponseCode.HTTPTimeout,

            HttpRequestException httpEx when httpEx.Message.Contains("429") => ResponseCode.HttpRE429,

            HttpRequestException httpEx when httpEx.Message.Contains("401") => ResponseCode.HttpRE401,

            _ => ResponseCode.GeneralExceptionError
        };

        return new ApiResponse
        {
            IsSuccess = false,
            StatusCode = code,
            ErrorMessage = ex.Message
        };
    }
}
