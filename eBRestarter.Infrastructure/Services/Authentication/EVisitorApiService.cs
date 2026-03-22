using eBRestarter.Core.Application.Interfaces.Authentication;
using eBRestarter.Core.Application.Interfaces.RestClient;
using eBRestarter.Core.Domain.Enums;
using eBRestarter.Core.Domain.Models;
using eBRestarter.Infrastructure.Constants;

namespace eBRestarter.Infrastructure.Services.Authentication;

public class EVisitorApiService(IRestClientService restClient) : IApiAuthenticationService
{
    private readonly IRestClientService _restClient = restClient;

    public async Task<(bool IsValid, string Message)> VerifyCredentialsAsync(string username, string apiKey)
    {
        // Wir bauen einen Test-Request (z.B. Account-Info abrufen)
        // Die URL muss natürlich zur eBesucher API passen.
        var request = new ApiRequest
        {
            Url = ApiWebLinks.HourlyEarnings, // Beispiel-Endpunkt!
            Username = username,
            Password = apiKey, // Bei eBesucher ist der API-Key oft das Passwort für Basic Auth
            TimeoutSeconds = 10
        };

        var response = await _restClient.ExecuteGetAsync(request);

        if (response.IsSuccess)
        {
            return (true, "Verbindung erfolgreich!");
        }
        else if (response.StatusCode == ResponseCode.HttpRE401)
        {
            return (false, "Benutzername oder API-Schlüssel falsch.");
        }
        else
        {
            return (false, $"Fehler: {response.ErrorMessage}");
        }
    }
}
