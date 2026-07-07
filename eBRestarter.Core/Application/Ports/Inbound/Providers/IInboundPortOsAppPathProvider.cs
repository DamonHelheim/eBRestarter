namespace eBRestarter.Core.Application.Ports.Inbound.Providers;

/// <summary>
/// Port: Provides OS-specific application directory and file paths to the application core and repositories.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND PORT (Driven Port / Steckdose)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt INNERHALB des Application Cores (<see cref="eBRestarter.Core.Application.UseCases.DownloadBrowserUseCase"/>) und im Infrastruktur-Layer.<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Infrastructure.Providers.WindowsAppPathProvider"/> in der Infrastruktur via OS-API).<br/>
/// - <strong>Begründung:</strong> Da die Implementierung auf betriebssystemspezifische Pfade zugreift und im Infrastruktur-Layer liegt, handelt es sich nach Leitfaden Abschnitt 1 zwingend um einen <strong>Outbound Port</strong>.<br/>
/// - <em>Architektur-Hinweis:</em> Befindet sich derzeit unter <c>Inbound/Providers</c>, gehört architektonisch jedoch in den Bereich <c>Outbound/OperatingSystem</c> (z. B. als <c>IOsAppPathProviderOutboundPort</c>).
/// </para>
/// </summary>
public interface IInboundPortOsAppPathProvider
{
    string RetrieveAppDataPath();
    string RetrieveDownloadsPath();
    string RetrieveConfigFilePath();
    string RetrieveLogFilePath();
}
