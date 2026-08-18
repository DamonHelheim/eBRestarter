using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RestSharp;
using RestSharp.Authenticators;

using eBRestarter.Core.Application.ObjectArchetypes.Constants;
using eBRestarter.Infrastructure.Api.Interfaces;
using eBRestarter.Infrastructure.ObjectArchetypes.Enums;
using eBRestarter.Infrastructure.ObjectArchetypes.Model;

namespace eBRestarter.Infrastructure.API;

/// <summary>
/// Adapter: Driven Adapter (Outbound Generic Gateway/Client) encapsulating HTTP communication via RestSharp.
/// <para>
/// <strong>Architectural Classification: OUTBOUND ADAPTER (Driven Adapter / Generic Outbound)</strong><br/>
/// - <strong>Role &amp; Responsibility:</strong> Fulfills requirements from the application core as a technological building block in the outer ring (Infrastructure Layer) by translating network requests into RestSharp invocations.<br/>
/// - <strong>Implemented Port:</strong> <see cref="IRestClient"/> (from the Application Core).<br/>
/// - <strong>Rationale:</strong> Positioned in the infrastructure layer, implementing an outbound port driven by the core to execute HTTP communication with external APIs.
/// </para>
/// </summary>
/// <param name="logger">The logger instance for recording API telemetry and error states.</param>
/// <param name="httpClientFactory">The HTTP client factory supplying pooled HTTP message handlers.</param>
public sealed partial class RestSharpClient(
    ILogger<RestSharpClient> logger,
    IHttpClientFactory httpClientFactory)
    : eBRestarter.Infrastructure.Api.Interfaces.IRestClient
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════

    // ── Block 2: Primitive Types & Strings (alphabetical) ──
    /// <summary>
    /// Name of the <see cref="IHttpClientFactory"/> registration backing every request of this client.
    /// </summary>
    /// <remarks>
    /// Tests can swap the transport without touching this class by registering
    /// <c>services.AddHttpClient(RestSharpClient.HttpClientName).ConfigurePrimaryHttpMessageHandler(() =&gt; fakeHandler)</c>.
    /// </remarks>
    public const string HttpClientName = "eBRestarter.RestSharp";

    private const string RateLimitRemainingHeaderName = "X-Ratelimit-Remaining";
    private const int RemainingRateLimitThreshold = 10;
    private const string RequestTimeoutErrorMessage = "The request has exceeded the time limit";


    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════

    // ── Block 1: Injected Dependencies (alphabetical) ──
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly ILogger<RestSharpClient> _logger = logger ?? throw new ArgumentNullException(nameof(logger));


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════

    /// <inheritdoc />
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
            LogSynchronousInvocationFailed(exception, request.Url);

            return MapExceptionToResponse(exception);
        }
    }

    /// <inheritdoc />
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
            LogAsynchronousInvocationFailed(exception, request.Url);

            return MapExceptionToResponse(exception);
        }
    }

    /// <summary>
    /// Checks the response headers for rate limiting thresholds and returns the corresponding response code.
    /// </summary>
    /// <param name="response">The RestSharp HTTP response.</param>
    /// <returns>A <see cref="ResponseCode"/> indicating if the rate limit threshold was reached or if the request succeeded.</returns>
    private static ResponseCode CheckRateLimit(RestResponse response)
    {
        var header = response.Headers?.FirstOrDefault(h => h.Name.Equals(RateLimitRemainingHeaderName, StringComparison.OrdinalIgnoreCase))?.Value?.ToString();

        if (int.TryParse(header, out int remaining) && remaining <= RemainingRateLimitThreshold)
        {
            return ResponseCode.RequestLimit;
        }

        return ResponseCode.Success;
    }

    /// <summary>
    /// Builds a <see cref="RestClient"/> on top of a pooled <see cref="HttpClient"/> from
    /// <see cref="IHttpClientFactory"/>.
    /// </summary>
    /// <param name="request">The API request containing target URL, timeout, and authentication credentials.</param>
    /// <remarks>
    /// <c>new RestClient(options)</c> was called here, which caused
    /// RestSharp to create a new <see cref="HttpMessageHandler"/> per request and dispose it on completion –
    /// leading to socket exhaustion and DNS stale issues.
    /// <para>
    /// <see cref="IHttpClientFactory.CreateClient(string)"/> provides a lightweight <see cref="HttpClient"/>
    /// wrapper over a SHARED message handler rotated every 5 minutes. Setting <c>BaseAddress</c> or <c>Timeout</c>
    /// on RestSharp affects only this instance wrapper without altering the pooled handler.
    /// </para>
    /// <para>
    /// <c>disposeHttpClient: false</c> is essential: the caller disposes the <see cref="RestClient"/> via <c>using</c>;
    /// without this flag, disposing the client wrapper would also dispose the shared handler and invalidate connection pooling.
    /// </para>
    /// </remarks>
    private RestClient CreateClient(ApiRequest request)
    {
        var options = new RestClientOptions(request.Url)
        {
            Timeout = TimeSpan.FromSeconds(request.TimeoutSeconds)
        };

        if (!string.IsNullOrWhiteSpace(request.Username) && !string.IsNullOrWhiteSpace(request.Password))
        {
            options.Authenticator = new HttpBasicAuthenticator(request.Username, request.Password);
        }

        HttpClient pooledHttpClient = _httpClientFactory.CreateClient(HttpClientName);

        return new RestClient(pooledHttpClient, options, disposeHttpClient: false);
    }

    /// <summary>
    /// Maps caught exceptions to standard <see cref="ApiResponse"/> error codes.
    /// </summary>
    /// <param name="exception">The encountered exception during HTTP execution.</param>
    /// <returns>An <see cref="ApiResponse"/> with failure status and mapped error code.</returns>
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

    /// <summary>
    /// Maps a RestSharp <see cref="RestResponse"/> to an <see cref="ApiResponse"/> result.
    /// </summary>
    /// <param name="response">The raw RestSharp response.</param>
    /// <returns>The mapped <see cref="ApiResponse"/>.</returns>
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
            LogRequestTimedOut(response.Request.Resource);

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

        LogApiErrorResponse(response.StatusCode, response.ErrorMessage);

        return new ApiResponse
        {
            IsSuccess = false,
            StatusCode = errorCode,
            ErrorMessage = response.ErrorMessage ?? response.StatusDescription
        };
    }


    // ═══════════════════════════════════════════════════════
    //  9. Log-Definitionen (Source-Generated)
    // ═══════════════════════════════════════════════════════
    // 📝 Logging Guidelines Section 3, Pattern B (Instance-based partial methods): These log events
    // are exclusively relevant to this class. Pattern B avoids boxing and runtime message template
    // parsing (Section 13) by evaluating IsEnabled checks prior to argument processing.

    [LoggerMessage(
        EventId = LogEventIds.Api.ApiRequestFailed,
        Level = LogLevel.Error,
        Message = "Error during synchronous API invocation: {Url}")]
    private partial void LogSynchronousInvocationFailed(Exception exception, string url);

    [LoggerMessage(
        EventId = LogEventIds.Api.ApiRequestFailed,
        Level = LogLevel.Error,
        Message = "Error during asynchronous API invocation: {Url}")]
    private partial void LogAsynchronousInvocationFailed(Exception exception, string url);

    [LoggerMessage(
        EventId = LogEventIds.Api.ApiRequestTimedOut,
        Level = LogLevel.Warning,
        Message = "API error: local timeout at {Resource}")]
    private partial void LogRequestTimedOut(string? resource);

    [LoggerMessage(
        EventId = LogEventIds.Api.ApiErrorResponse,
        Level = LogLevel.Warning,
        Message = "API error: {StatusCode} - {ErrorMessage}")]
    private partial void LogApiErrorResponse(HttpStatusCode statusCode, string? errorMessage);
}
