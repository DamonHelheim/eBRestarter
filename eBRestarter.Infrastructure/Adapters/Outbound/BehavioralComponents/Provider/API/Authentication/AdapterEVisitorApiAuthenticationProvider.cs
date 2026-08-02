using System;
using System.Threading.Tasks;

using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Authentication;
using eBRestarter.Infrastructure.Api;
using eBRestarter.Infrastructure.Api.Interfaces;
using eBRestarter.Infrastructure.ObjectArchetypes.Enums;
using eBRestarter.Infrastructure.ObjectArchetypes.Model;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Provider.API.Authentication;

/// <summary>
/// Adapter: Driven Adapter (Outbound Provider) for verifying user credentials against the eBesucher API.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter / Provider)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Dienstleister im äußeren Ring (Infrastructure Layer) die externe Authentifizierungsprüfung via HTTP-REST.<br/>
/// - <strong>Implementierter Port:</strong> <see cref="IOutboundPortApiAuthenticationProvider"/> (aus dem Application Core).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein vorbildlicher <strong>Outbound Adapter</strong> (Taxonomie: Provider Adapter), da sie im Infrastructure-Layer liegt, einen Outbound Port implementiert und vom Core angetrieben wird, um technologische API-Loginprüfungen auszuführen.
/// </para>
/// </summary>
public sealed class AdapterEVisitorApiAuthenticationProvider : IOutboundPortApiAuthenticationProvider
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitive Typen & Strings ──
    private const string ConnectionSuccessfulMessage = "Connection successful!";
    private const string ErrorPrefix = "Error: ";
    private const string InvalidCredentialsErrorMessage = "Invalid username or API key.";
    private const int RequestTimeoutSeconds = 10;

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    // ── Block 1: Injizierte Abhängigkeiten (Dependencies) ──
    private readonly IRestClient _restClientPort;


    // ═══════════════════════════════════════════════════════
    //  6. Constructors
    // ═══════════════════════════════════════════════════════
    public AdapterEVisitorApiAuthenticationProvider(IRestClient restClientPort)
    {
        ArgumentNullException.ThrowIfNull(restClientPort);

        _restClientPort = restClientPort;
    }


    // ═══════════════════════════════════════════════════════
    //  8. Methods (public → private)
    // ═══════════════════════════════════════════════════════
    public Task<VerificationResult> VerifyCredentialsAsync(string username, string apiKey)
    {
        //Immediate Guard-Clause Exception Timing (Guide Abs. 9.1)
        ArgumentNullException.ThrowIfNull(username);
        ArgumentNullException.ThrowIfNull(apiKey);

        return VerifyCredentialsCoreAsync(username, apiKey);
    }

    private async Task<VerificationResult> VerifyCredentialsCoreAsync(string username, string apiKey)
    {
        var request = new ApiRequest
        {
            Url = ApiWebLinks.HourlyEarnings,
            Username = username,
            Password = apiKey,
            TimeoutSeconds = RequestTimeoutSeconds
        };

        var response = await _restClientPort.ExecuteGetAsync(request).ConfigureAwait(false);

        return response switch
        {
            { IsSuccess: true } => new VerificationResult(true, ConnectionSuccessfulMessage),
            { StatusCode: ResponseCode.HttpRE401 } => new VerificationResult(false, InvalidCredentialsErrorMessage),
            _ => new VerificationResult(false, $"{ErrorPrefix}{response.ErrorMessage}")
        };
    }
}
