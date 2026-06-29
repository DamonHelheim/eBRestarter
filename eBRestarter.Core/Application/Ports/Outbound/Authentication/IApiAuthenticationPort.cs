namespace eBRestarter.Core.Application.Ports.Outbound.Authentication;

using eBRestarter.Core.Application.Models.Records;

public interface IApiAuthenticationPort
{
    // Validates whether the username/key credentials are valid (invokes the eBesucher API)
    Task<VerificationResult> VerifyCredentialsAsync(string username, string apiKey);
}

