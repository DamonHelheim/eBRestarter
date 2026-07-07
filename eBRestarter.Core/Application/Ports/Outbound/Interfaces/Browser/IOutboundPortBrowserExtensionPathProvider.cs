namespace eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;

/// <summary>
/// Port: Driven Port (Outbound) for resolving the filesystem path of the unpacked browser extension on the host machine.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND PORT (Driven Port / Steckdose für OS-Pfadauflösung)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt in ViewModels (<see cref="eBRestarter.Desktop.WinUI3.ViewModels.ViewModelOptionsExtension"/>) sowie bei der Browser-Startkonfiguration.<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Infrastructure.Adapters.WindowsOS.WindowsBrowserExtensionDeploymentAdapter"/> im Infrastructure Layer via OS-Verzeichnisse).<br/>
/// - <strong>Begründung:</strong> Kapselt die Ermittlung physischer OS-Verzeichnispfade und ist daher nach Abschnitt 1 des Leitfadens ein klassischer <strong>Outbound Port</strong>.<br/>
/// - <em>Architektur-Hinweis (Gelöst):</em> Der Namespace wurde passend zum Ordner auf <c>eBRestarter.Core.Application.Ports.Outbound.Browser</c> aktualisiert.
/// </para>
/// </summary>
public interface IOutboundPortBrowserExtensionPathProvider
{
    /// <summary>
    /// Returns the current path to the extension folder (Debug vs. Release).
    /// </summary>
    /// <returns>The absolute path to the extension folder.</returns>
    string RetrieveExtensionFolderPath();
}
