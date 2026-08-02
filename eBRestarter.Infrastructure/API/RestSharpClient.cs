using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RestSharp;
using RestSharp.Authenticators;

using eBRestarter.Infrastructure.Api.Interfaces;
using eBRestarter.Infrastructure.ObjectArchetypes.Enums;
using eBRestarter.Infrastructure.ObjectArchetypes.Model;

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
public sealed class RestSharpClient(
    ILogger<RestSharpClient> logger,
    HttpMessageHandler? httpMessageHandler = null)
    : eBRestarter.Infrastructure.Api.Interfaces.IRestClient
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════

    // ── Block 2: Primitive Typen & Strings (alphabetisch) ──
    private const string RateLimitRemainingHeaderName = "X-Ratelimit-Remaining";
    private const int RemainingRateLimitThreshold = 10;
    private const string RequestTimeoutErrorMessage = "The request has exceeded the time limit";


    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════

    // ── Block 1: Injizierte Abhängigkeiten (alphabetisch) ──
    private readonly ILogger<RestSharpClient> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly HttpMessageHandler? _httpMessageHandler = httpMessageHandler;


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════

    public ApiResponse ExecuteGet(ApiRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var client = CreateClient(request);

        var restRequest = new RestRequest();

        try
        {
            var response = client.ExecuteGet(restRequest);
            return MapResponse(response);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error during synchronous API invocation: {Url}", request.Url);

            return MapExceptionToResponse(exception);
        }
    }

    public async Task<ApiResponse> ExecuteGetAsync(ApiRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var client = CreateClient(request);

        var restRequest = new RestRequest();

        try
        {
            var response = await client.ExecuteGetAsync(restRequest);

            return MapResponse(response);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error during asynchronous API invocation: {Url}", request.Url);

            return MapExceptionToResponse(exception);
        }
    }

    private static ResponseCode CheckRateLimit(RestResponse response)
    {
        var header = response.Headers?.FirstOrDefault(h => h.Name.Equals(RateLimitRemainingHeaderName, StringComparison.OrdinalIgnoreCase))?.Value?.ToString();

        if (int.TryParse(header, out int remaining) && remaining <= RemainingRateLimitThreshold)
        {
            return ResponseCode.RequestLimit;
        }

        return ResponseCode.Success;
    }

    private RestClient CreateClient(ApiRequest request)
    {
        var options = new RestClientOptions(request.Url)
        {
            Timeout = TimeSpan.FromSeconds(request.TimeoutSeconds)
        };

        if (_httpMessageHandler is not null)
        {
            options.ConfigureMessageHandler = _ => _httpMessageHandler;
        }

        if (!string.IsNullOrWhiteSpace(request.Username) && !string.IsNullOrWhiteSpace(request.Password))
        {
            options.Authenticator = new HttpBasicAuthenticator(request.Username, request.Password);
        }

        return new RestClient(options);
    }

    private static ApiResponse MapExceptionToResponse(Exception exception)
    {
        var code = exception switch
        {
            TimeoutException or TaskCanceledException => ResponseCode.HTTPTimeout,
            HttpRequestException httpException when httpException.StatusCode == HttpStatusCode.TooManyRequests || httpException.Message.Contains(((int)HttpStatusCode.TooManyRequests).ToString(), StringComparison.OrdinalIgnoreCase) => ResponseCode.HttpRE429,
            HttpRequestException httpException when httpException.StatusCode == HttpStatusCode.Unauthorized || httpException.Message.Contains(((int)HttpStatusCode.Unauthorized).ToString(), StringComparison.OrdinalIgnoreCase) => ResponseCode.HttpRE401,
            _ => ResponseCode.GeneralExceptionError
        };

        return new ApiResponse
        {
            IsSuccess = false,
            StatusCode = code,
            ErrorMessage = exception.Message
        };
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

        if (response.ResponseStatus == ResponseStatus.TimedOut)
        {
            _logger.LogWarning("API error: Local timeout at {Url}", response.Request.Resource);

            return new ApiResponse
            {
                IsSuccess = false,
                StatusCode = ResponseCode.HTTPTimeout,
                ErrorMessage = RequestTimeoutErrorMessage
            };
        }

        var errorCode = response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => ResponseCode.HttpRE401,
            HttpStatusCode.TooManyRequests => ResponseCode.HttpRE429,
            HttpStatusCode.InternalServerError => ResponseCode.InternalServerError,
            HttpStatusCode.RequestTimeout => ResponseCode.HTTPTimeout,
            HttpStatusCode.GatewayTimeout => ResponseCode.HTTPTimeout,
            _ => ResponseCode.NoConnectionToServer
        };

        _logger.LogWarning("API Error: {Status} - {Msg}", response.StatusCode, response.ErrorMessage);

        return new ApiResponse
        {
            IsSuccess = false,
            StatusCode = errorCode,
            ErrorMessage = response.ErrorMessage ?? response.StatusDescription
        };
    }
}
