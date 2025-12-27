using eBRestarter.Core.Application.Interfaces.RestClient;
using eBRestarter.Core.Domain.Enums;
using eBRestarter.Core.Domain.Models;
using Microsoft.Extensions.Logging;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace eBRestarter.Infrastructure.Services.RestSharp
{
    public class RestSharpClientService : IRestClientService
    {
        private readonly ILogger<RestSharpClientService> _logger;

        public RestSharpClientService(ILogger<RestSharpClientService> logger)
        {
            _logger = logger;
        }

        public async Task<ApiResponse> ExecuteGetAsync(ApiRequest requestModel)
        {
            // Client erstellen (using sorgt für Dispose)
            using var client = CreateClient(requestModel);
            var request = new RestRequest();

            try
            {
                var response = await client.ExecuteGetAsync(request);
                return MapResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim asynchronen API-Aufruf: {Url}", requestModel.Url);
                return MapExceptionToResponse(ex);
            }
        }

        public ApiResponse ExecuteGet(ApiRequest requestModel)
        {
            using var client = CreateClient(requestModel);
            var request = new RestRequest();

            try
            {
                var response = client.ExecuteGet(request);
                return MapResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim synchronen API-Aufruf: {Url}", requestModel.Url);
                return MapExceptionToResponse(ex);
            }
        }

        // --- Helper Methoden ---

        private RestClient CreateClient(ApiRequest model)
        {
            var options = new RestClientOptions(model.Url)
            {
                Timeout = TimeSpan.FromSeconds(model.TimeoutSeconds)
            };

            // Vereinfachte Logik: Authenticator nur setzen, wenn beide Werte da sind.
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
                // Rate Limit Check (Logik extrahiert)
                var rateCode = CheckRateLimit(response);

                return new ApiResponse
                {
                    IsSuccess = true,
                    Content = response.Content,
                    StatusCode = rateCode // Kann Success oder RequestLimit sein
                };
            }

            // 2. HTTP Fehler Mapping (Status Code -> Dein Enum)
            var errorCode = response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => ResponseCode.HttpRE401,
                HttpStatusCode.TooManyRequests => ResponseCode.HttpRE429,
                HttpStatusCode.InternalServerError => ResponseCode.InternalServerError,
                HttpStatusCode.RequestTimeout => ResponseCode.HTTPTimeout,
                HttpStatusCode.GatewayTimeout => ResponseCode.HTTPTimeout, // 504 auch als Timeout behandeln
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

        private ResponseCode CheckRateLimit(RestResponse response)
        {
            // Versuche den Header sicher zu lesen
            var header = response.Headers?.FirstOrDefault(h => h.Name == "X-Ratelimit-Remaining")?.Value?.ToString();

            if (int.TryParse(header, out int remaining) && remaining <= 10)
            {
                return ResponseCode.RequestLimit;
            }
            return ResponseCode.Success;
        }

        private ApiResponse MapExceptionToResponse(Exception ex)
        {
            // Mapping von .NET Exceptions zu deinem Enum
            var code = ex switch
            {
                TimeoutException => ResponseCode.HTTPTimeout,
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
}
