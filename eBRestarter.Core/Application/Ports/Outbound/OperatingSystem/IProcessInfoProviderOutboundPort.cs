namespace eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;

/// <summary>
/// Port: Driven Port (Outbound) for retrieving physical runtime metadata of the currently running host process (e.g., executable binary path).
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND PORT (Driven Port / Steckdose für OS-Prozessmetadaten)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt in Infrastruktur-Repositories wie <see cref="eBRestarter.Infrastructure.Adapters.WindowsOS.WindowsStartupRepository"/> zur Ermittlung des eigenen Exe-Pfades für Autostart-Einträge.<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Infrastructure.Adapters.Wrapper.ProcessInfoProvider"/> via .NET-Laufzeitsystem).<br/>
/// - <strong>Begründung:</strong> Abstrahiert die Ermittlung des physischen Dateipfades der laufenden Anwendung und entkoppelt dies vom Core, was nach Leitfaden einen klassischen <strong>Outbound Port</strong> darstellt.<br/>
/// - <em>Architektur-Hinweis:</em> Suffix <c>OutboundPort</c> ist perfekt. Trennt nach ISP sauber die Prozess-Metadaten von Anwendungs-Verzeichnispfaden (<see cref="eBRestarter.Core.Application.Ports.Outbound.Providers.IAppPathProviderOutboundPort"/>).
/// </para>
/// </summary>
public interface IProcessInfoProviderOutboundPort
{
    string GetCurrentExecutablePath();
}


