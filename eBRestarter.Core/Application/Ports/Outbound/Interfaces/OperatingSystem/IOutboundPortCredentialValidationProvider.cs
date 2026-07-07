namespace eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;

/// <summary>
/// Port: Driven Port (Outbound) for validating local Windows or Active Directory account credentials (e.g., for Auto-Logon).
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND PORT (Driven Port / Steckdose für OS-Anmeldevalidierung)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt im Application Core (<see cref="UseCases.ConfigureAutoLogonUseCase"/>).<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Infrastructure.Adapters.Authentication.WindowsCredentialValidationProvider"/> via Win32 API oder Active Directory).<br/>
/// - <strong>Begründung:</strong> Kapselt die OS-spezifische Überprüfung von Benutzerkonten für den Anwendungskern und ist somit nach Leitfaden ein klassischer <strong>Outbound Port</strong>.<br/>
/// - <em>Architektur-Hinweis:</em> Suffix <c>OutboundPort</c> ist vorbildlich.
/// </para>
/// </summary>
public interface IOutboundPortCredentialValidationProvider
{
    /// <summary>
    /// Validiert Benutzername/Domain/Passwort gegen den lokalen Rechner oder die Domain.
    /// </summary>
    /// <param name="username">Benutzername</param>
    /// <param name="domain">Domain oder Rechnername</param>
    /// <param name="password">Passwort</param>
    /// <returns>true wenn gültig, sonst false. Kann bei Domain-Fehlern Exception werfen.</returns>
    bool ValidateCredentials(string username, string domain, string password);
}



