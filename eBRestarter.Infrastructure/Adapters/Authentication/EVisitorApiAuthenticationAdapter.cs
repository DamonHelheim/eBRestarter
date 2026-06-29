using eBRestarter.Infrastructure.Api;
using eBRestarter.Infrastructure.Browser;
using eBRestarter.Infrastructure.OperatingSystem;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Ports.Outbound.Authentication;
using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Infrastructure.Adapters.RestSharp;
using eBRestarter.Infrastructure.Network;
using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Infrastructure.Network;

namespace eBRestarter.Infrastructure.Adapters.Authentication;

public sealed class EVisitorApiAuthenticationAdapter(IRestClientPort restClientUseCase) : IApiAuthenticationPort
{
    private readonly IRestClientPort _restClientUseCase = restClientUseCase;

    public async Task<VerificationResult> VerifyCredentialsAsync(string username, string apiKey)
    {
        // We build a test request (e.g., retrieving account info).
        // The URL must match the eBesucher API endpoint.
        var request = new ApiRequest
        {
            Url = ApiWebLinks.HourlyEarnings, // Example endpoint!
            Username = username,
            Password = apiKey, // For eBesucher, the API key is often used as the password for Basic Auth
            TimeoutSeconds = 10
        };

        var response = await _restClientUseCase.ExecuteGetAsync(request);

        if (response.IsSuccess)
        {
            return new VerificationResult(true, "Connection successful!");
        }
        else if (response.StatusCode == ResponseCode.HttpRE401)
        {
            return new VerificationResult(false, "Invalid username or API key.");
        }
        else
        {
            return new VerificationResult(false, $"Error: {response.ErrorMessage}");
        }
    }
}




