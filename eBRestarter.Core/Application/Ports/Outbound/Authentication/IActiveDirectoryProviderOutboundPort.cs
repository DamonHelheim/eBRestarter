using eBRestarter.Core.Application.Enums;

namespace eBRestarter.Core.Application.Ports.Outbound.Authentication;

/// <summary>
/// Port: Driven Port (Outbound) for validating user credentials against Windows Active Directory / SAM account database.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND PORT (Driven Port / Steckdose für OS-Authentifizierung)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt in Infrastruktur-Adaptern (<see cref="eBRestarter.Infrastructure.Adapters.Authentication.WindowsCredentialValidationProvider"/>).<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Infrastructure.Adapters.Authentication.WindowsActiveDirectoryProvider"/> via <c>PrincipalContext</c>).<br/>
/// - <strong>Begründung:</strong> Kapselt die externe Anbindung an das Windows Active Directory und ist somit nach Leitfaden Abschnitt 1 ein <strong>Outbound Port</strong>.<br/>
/// - <em>Architektur-Hinweis (Gelöst):</em> Die ursprüngliche Abhängigkeitsverletzung (direkter Import von <c>System.DirectoryServices.AccountManagement</c> im Application Core) wurde durch das domäneneigene Enum <see cref="DirectoryContextScope"/> aufgelöst.
/// </para>
/// </summary>
public interface IActiveDirectoryProviderOutboundPort
{
    /// <summary>
    /// Validates the user credentials against Active Directory.
    /// This is a read-only query that checks an external state and returns a yes/no result
    /// without modifying any data within our own system, making it a clean Provider.
    /// </summary>
    bool ValidateCredentials(DirectoryContextScope contextScope, string domain, string username, string password);
}

