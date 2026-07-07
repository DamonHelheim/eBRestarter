namespace eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;

/// <summary>
/// Port: Driven Port (Outbound) for inspecting and modifying OS-level browser optimization settings (e.g., Microsoft Edge Startup Boost).
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND PORT (Driven Port / Steckdose für Browser-OS-Konfiguration)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt im Application Core (<see cref="UseCases.ToggleEdgeStartupBoostUseCase"/>).<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Infrastructure.Adapters.WindowsOS.WindowsStartupRepository"/> via Windows-Registry).<br/>
/// - <strong>Begründung:</strong> Kapselt OS-spezifische Konfigurationszugriffe (z. B. Edge Startup Boost) für den Anwendungskern und ist somit nach Leitfaden ein klassischer <strong>Outbound Port</strong>.<br/>
/// - <em>Architektur-Hinweis:</em> Suffix <c>RepositoryOutboundPort</c> ist vorbildlich.
/// </para>
/// </summary>
public interface IOutboundPortBrowserConfigRepository
{
    void SetBrowserStartupBoost(bool enable);
    bool IsBrowserStartupBoostEnabled();
}
