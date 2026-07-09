using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Authentication;
using eBRestarter.Infrastructure.Api;
using eBRestarter.Infrastructure.Api.Interfaces;
using eBRestarter.Infrastructure.Enums;
using eBRestarter.Infrastructure.ObjectArchetypes.Model;

namespace eBRestarter.Infrastructure.Adapters.Outbound.API.Authentication;

/// <summary>
/// Adapter: Driven Adapter (Outbound Provider) for verifying user credentials against the eBesucher API.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter / Provider)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Dienstleister im äußeren Ring (Infrastructure Layer) die externe Authentifizierungsprüfung via HTTP-REST.<br/>
/// - <strong>Implementierter Port:</strong> <see cref="IOutboundPortApiAuthenticationProvider"/> (aus dem Application Core).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein vorbildlicher <strong>Outbound Adapter</strong> (Taxonomie: Provider Adapter), da sie im Infrastructure-Layer liegt, einen Outbound Port implementiert und vom Core angetrieben wird, um technologische API-Loginprüfungen auszuführen.
/// </para>
/// </summary>
public sealed class AdapterEVisitorApiAuthenticationProvider(IRestClient restClientPort) : IOutboundPortApiAuthenticationProvider
{
    private readonly IRestClient _restClientPort = restClientPort;

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

        var response = await _restClientPort.ExecuteGetAsync(request);

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




