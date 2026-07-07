namespace eBRestarter.Core.Application.Ports.Outbound.Application;

/// <summary>
/// Port: Driven Port (Outbound) for resolving standard operating system filesystem paths (AppData, LocalAppData, ProgramFiles etc.).
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND PORT (Driven Port / Steckdose für OS-Pfadsystem)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt in Repositories und Adaptern zur Ermittlung systemspezifischer Speicherorte.<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Infrastructure.Adapters.Providers.WindowsOS.WindowsAppPathProvider"/> via <c>Environment.GetFolderPath</c>).<br/>
/// - <strong>Begründung:</strong> Da diese Schnittstelle die Abstraktion physischer OS-Verzeichnisse für die Anwendung darstellt, handelt es sich nach Abschnitt 1 des Leitfadens um einen klassischen <strong>Outbound Port</strong>.<br/>
/// - <em>Architektur-Hinweis:</em> Suffix <c>OutboundPort</c> ist vorbildlich. Zur thematischen Präzisierung könnte die Datei zukünftig in <c>Ports/Outbound/OperatingSystem/</c> verschoben werden.
/// </para>
/// </summary>
public interface IAppPathProviderOutboundPort
{
    string RetrieveAppDataDirectory();
    string RetrieveLocalAppDataDirectory();
    string RetrieveUserProfileDirectory();
    string RetrieveProgramFilesDirectory();
    string RetrieveProgramFilesX86Directory();
}


