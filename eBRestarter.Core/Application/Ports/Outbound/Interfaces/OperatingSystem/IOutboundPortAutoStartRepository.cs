namespace eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;

/// <summary>
/// Port: Driven Port (Outbound) for managing application startup entries in the host operating system (e.g., Windows Registry or Startup folder).
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND PORT (Driven Port / Steckdose für OS-Autostart)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt im Application Core (<see cref="UseCases.ToggleAppAutoStartUseCase"/>) und in Update-Adaptern.<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Infrastructure.Adapters.WindowsOS.WindowsStartupRepository"/> via Windows-Registry/Verknüpfungen).<br/>
/// - <strong>Begründung:</strong> Kapselt die physische Konfiguration des Betriebssystem-Autostarts für den Core und ist somit nach Leitfaden ein klassischer <strong>Outbound Port</strong>.<br/>
/// - <em>Architektur-Hinweis:</em> Suffix <c>RepositoryOutboundPort</c> ist vorbildlich.
/// </para>
/// </summary>
public interface IOutboundPortAutoStartRepository
{
    Task EnableAutoStartAsync();
    Task DisableAutoStartAsync();
    Task<bool> IsAutoStartEnabledAsync();
    void EnableAutoStart();
    void DisableAutoStart();
    Dictionary<string, object> RetrieveStartupEntries();
}
