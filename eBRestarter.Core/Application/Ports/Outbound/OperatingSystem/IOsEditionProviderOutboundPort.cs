namespace eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;

/// <summary>
/// Port: Driven Port (Outbound) for querying detailed operating system edition and version string information via WMI or system APIs.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND PORT (Driven Port / Steckdose für OS-Editionsabfrage)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt im Application Core (<see cref="eBRestarter.Core.Application.Providers.SystemInformationProvider"/>).<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Infrastructure.Adapters.WmiHardwareProvider"/> via Windows WMI).<br/>
/// - <strong>Begründung:</strong> Kapselt spezifische OS-Abfragen für den Anwendungskern und ist nach Leitfaden ein klassischer <strong>Outbound Port</strong>.<br/>
/// - <em>Architektur-Hinweis:</em> Suffix <c>OutboundPort</c> ist vorbildlich. Trennt nach dem Interface Segregation Principle (ISP) sauber die Editions-Abfrage von der Hardware-Abfrage (<see cref="IHardwareInfoProviderOutboundPort"/>).
/// </para>
/// </summary>
public interface IOsEditionProviderOutboundPort
{
    Task<string> RetrieveOsEditionAsync();
}
