using eBRestarter.Core.Application.Interfaces.Authentication;
using eBRestarter.Core.Application.Interfaces.RestClient;
using eBRestarter.Core.Domain.Enums;
using eBRestarter.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Infrastructure.Services
{
    public class EBesucherApiService : IApiAuthenticationService
    {
        private readonly IRestClientService _restClient;

        public EBesucherApiService(IRestClientService restClient)
        {
            _restClient = restClient;
        }

        public async Task<(bool IsValid, string Message)> VerifyCredentialsAsync(string username, string apiKey)
        {
            // Wir bauen einen Test-Request (z.B. Account-Info abrufen)
            // Die URL muss natürlich zur eBesucher API passen.
            var request = new ApiRequest
            {
                Url = "https://www.ebesucher.de/api/visitor_exchange.json/account/status", // Beispiel-Endpunkt!
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
}
