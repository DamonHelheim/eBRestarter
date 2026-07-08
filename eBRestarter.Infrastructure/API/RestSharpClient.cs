using eBRestarter.Infrastructure.Api.Interfaces;
using eBRestarter.Infrastructure.Enums;
using eBRestarter.Infrastructure.Models.Network;
using eBRestarter.Infrastructure.ObjectArchetypes.Model;
using Microsoft.Extensions.Logging;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;

namespace eBRestarter.Infrastructure.API;

/// <summary>
/// Adapter: Driven Adapter (Outbound Generic Gateway/Client) encapsulating HTTP communication via RestSharp.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter / Generic Outbound)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Baustein im äußeren Ring (Infrastructure Layer) Vorgaben aus dem Core durch Übersetzung von Netzwerk-Requests in RestSharp-Aufrufe.<br/>
/// - <strong>Implementierter Port:</strong> <see cref="IRestClient"/> (aus dem Application Core).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein <strong>Outbound Adapter</strong>, da sie im Infrastructure-Layer liegt, einen Outbound Port implementiert und vom Core angetrieben wird, um HTTP-Kommunikation mit externen APIs auszuführen.
/// </para>
/// </summary>
public sealed class RestSharpClient(ILogger<RestSharpClient> logger, HttpMessageHandler? httpMessageHandler = null) : Api.Interfaces.IRestClient
{
    private readonly ILogger<RestSharpClient> _logger = logger;
    private readonly HttpMessageHandler? _httpMessageHandler = httpMessageHandler;

    public async Task<ApiResponse> ExecuteGetAsync(ApiRequest request)
    {
        using var client = CreateClient(request);

        var restRequest = new RestRequest();

        try
        {
            var response = await client.ExecuteGetAsync(restRequest);

            return MapResponse(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during asynchronous API invocation: {Url}", request.Url);

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
            _logger.LogError(ex, "Error during synchronous API invocation: {Url}", request.Url);

            return MapExceptionToResponse(ex);
        }
    }

    private RestClient CreateClient(ApiRequest model)
    {
        var options = new RestClientOptions(model.Url)
        {
            Timeout = TimeSpan.FromSeconds(model.TimeoutSeconds)
        };

        // Supplying the RestClient with the mock test handler if present
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

        // At this stage, response.StatusCode is typically '0' for client timeouts
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

        // 2. HTTP Error Mapping (Status Code -> Custom Application Enum)
        var errorCode = response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => ResponseCode.HttpRE401,
            HttpStatusCode.TooManyRequests => ResponseCode.HttpRE429,
            HttpStatusCode.InternalServerError => ResponseCode.InternalServerError,
            HttpStatusCode.RequestTimeout => ResponseCode.HTTPTimeout, // Server reports timeout
            HttpStatusCode.GatewayTimeout => ResponseCode.HTTPTimeout,
            _ => ResponseCode.NoConnectionToServer // Fallback threshold
        };

        _logger.LogWarning("API Error: {Status} - {Msg}", response.StatusCode, response.ErrorMessage);

        return new ApiResponse
        {
            IsSuccess = false,
            StatusCode = errorCode,
            ErrorMessage = response.ErrorMessage ?? response.StatusDescription
        };
    }

    private static ResponseCode CheckRateLimit(RestResponse response)
    {
        // Attempt to safely retrieve the specific header value
        var header = response.Headers?.FirstOrDefault(h => h.Name == "X-Ratelimit-Remaining")?.Value?.ToString();

        if (int.TryParse(header, out int remaining) && remaining <= 10)
        {
            return ResponseCode.RequestLimit;
        }

        return ResponseCode.Success;
    }

    private static ApiResponse MapExceptionToResponse(Exception ex)
    {
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




