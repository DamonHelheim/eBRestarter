namespace eBRestarter.Core.Application.Ports.Outbound.Browser;

/// <summary>
/// Port: Driven Port (Outbound) for deploying and copying browser extension files onto the operating system filesystem.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND PORT (Driven Port / Steckdose für OS-Dateisystem/Deployment)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt in ViewModels (<see cref="eBRestarter.Desktop.WinUI3.ViewModels.ViewModelOptionsExtension"/>) sowie bei der Hintergrund-Initialisierung.<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Infrastructure.Adapters.WindowsOS.WindowsBrowserExtensionDeploymentAdapter"/> im Infrastructure Layer via OS-Dateioperationen).<br/>
/// - <strong>Begründung:</strong> Da die Implementierung physische Datei-Kopieroperationen im Betriebssystem ausführt, handelt es sich nach Abschnitt 1 des Leitfadens um einen vorbildlichen <strong>Outbound Port</strong>.<br/>
/// - <em>Architektur-Hinweis:</em> Suffix <c>OutboundPort</c> ist perfekt. Der alte Kommentar ("Can remain declared as a HANDLER") war irreführend und architektonisch inakkurat.
/// </para>
/// </summary>
public interface IBrowserExtensionDeploymentOutboundPort
{

    /// <summary>
    /// Ensures that the Chrome extension is located in the correct target directory (e.g., AppData).
    /// Copies the files if they do not exist there yet.
    /// </summary>
    void EnsureExtensionIsDeployed();
}
