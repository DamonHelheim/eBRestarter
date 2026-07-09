namespace eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;

/// <summary>
/// Port: Driven Port (Outbound) for managing automatic user logon configurations in the operating system (e.g., Windows Winlogon Registry keys).
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND PORT (Driven Port / Steckdose für OS-AutoLogon-Konfiguration)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt im Application Core (<see cref="UseCases.ConfigureAutoLogonUseCase"/>) und in GUI-ViewModels.<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Infrastructure.Adapters.WindowsOS.Security.WindowsAutoLogonRepository"/> via Windows Registry/LSA Secrets).<br/>
/// - <strong>Begründung:</strong> Kapselt sensible OS-spezifische Anmeldekonfigurationen für den Anwendungskern und ist nach Leitfaden ein klassischer <strong>Outbound Port</strong>.<br/>
/// - <em>Architektur-Hinweis:</em> Suffix <c>RepositoryOutboundPort</c> ist vorbildlich.
/// </para>
/// </summary>
public interface IOutboundPortOsAutoLogonRepository
{
    void EnableAutoLogon(string username, string domain, string password);
    bool IsPasswordlessAuthEnabled();
    void SetPasswordlessAuth(bool enable);
    void DisableAutoLogon();
    bool IsAutoLogonEnabled();
}
