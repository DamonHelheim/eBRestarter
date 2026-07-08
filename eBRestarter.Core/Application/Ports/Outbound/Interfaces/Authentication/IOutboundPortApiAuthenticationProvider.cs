using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

namespace eBRestarter.Core.Application.Ports.Outbound.Authentication;

/// <summary>
/// Port: Driven Port (Outbound) for verifying user API credentials against the remote E-Visitor web service via HTTP.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND PORT (Driven Port / Steckdose für Remote-API-Auth)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt in der Benutzeroberfläche (<see cref="eBRestarter.Desktop.WinUI3.ViewModels.ViewModelActivateApi"/> via MVVM).<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Infrastructure.Adapters.Authentication.EVisitorApiAuthenticationProvider"/> im Infrastructure Layer via HTTP-REST-Client).<br/>
/// - <strong>Begründung:</strong> Da diese Schnittstelle im Core definiert ist, um die Validierung von Zugangsdaten an einen externen Webservice auszulagern, handelt es sich nach Abschnitt 1 des Leitfadens um einen vorbildlichen <strong>Outbound Port</strong>.<br/>
/// - <em>Architektur-Hinweis:</em> Suffix <c>OutboundPort</c> ist perfekt.
/// </para>
/// </summary>
public interface IOutboundPortApiAuthenticationProvider
{
    // Validates whether the username/key credentials are valid (invokes the eBesucher API)
    Task<VerificationResult> VerifyCredentialsAsync(string username, string apiKey);
}

