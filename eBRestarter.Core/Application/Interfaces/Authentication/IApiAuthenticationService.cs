namespace eBRestarter.Core.Application.Interfaces.Authentication;

public interface IApiAuthenticationService
{
    // Prüft, ob Username/Key gültig sind (ruft eBesucher API auf)
    Task<(bool IsValid, string Message)> VerifyCredentialsAsync(string username, string apiKey);
}
