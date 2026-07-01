using eBRestarter.Core.Application.Models.Records;

namespace eBRestarter.Core.Application.Ports.Outbound.Authentication;

public interface IApiAuthenticationProviderOutboundPort
{
    // Validates whether the username/key credentials are valid (invokes the eBesucher API)
    Task<VerificationResult> VerifyCredentialsAsync(string username, string apiKey);
}

