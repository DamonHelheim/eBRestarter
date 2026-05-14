namespace eBRestarter.Core.Application.Interfaces.Authentication;

public record VerificationResult(bool IsValid, string Message);

public interface IApiAuthenticationService
{
    // Prüft, ob Username/Key gültig sind (ruft eBesucher API auf)
    Task<VerificationResult> VerifyCredentialsAsync(string username, string apiKey);
}
