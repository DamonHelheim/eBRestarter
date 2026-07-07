using eBRestarter.Core.Application.Models;

namespace eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;

/// <summary>
/// Port: Driven Port (Outbound) for inspecting the operating system and discovering installed Web Browsers and their metadata.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND PORT (Driven Port / Steckdose für OS-Browsererkennung)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt im Application Core und in der Präsentationsschicht zur Ermittlung installierter Webbrowser.<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Infrastructure.Adapters.WindowsOS.WindowsBrowserDiscoveryProvider"/> via Windows-Registry und Dateisystem-Suche).<br/>
/// - <strong>Begründung:</strong> Kapselt die physische Suche nach ausführbaren Browser-Dateien und Registry-Schlüsseln im OS und ist somit nach Abschnitt 1 des Leitfadens ein vorbildlicher <strong>Outbound Port</strong>.<br/>
/// - <em>Architektur-Hinweis:</em> Suffix <c>OutboundPort</c> ist perfekt.
/// </para>
/// </summary>
public interface IOutboundPortBrowserDiscoveryProvider
{
    /// <summary>
    /// Performs a read-only system search and returns browser metadata to the Core.
    /// Note: The original suffix was redundant (ServiceProvider). The name has been cleaned up to be a pure Provider.
    /// </summary>
    Task<IEnumerable<BrowserInfo>> FindInstalledBrowsersAsync();
}

