using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models.Records;

namespace eBRestarter.Core.Application.Ports.Outbound.Browser;

/// <summary>
/// Port: Driven Port (Outbound) defining the capabilities for inspecting, launching, controlling, and terminating concrete OS Web Browsers.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND PORT (Driven Port / Steckdose für Browser-Steuerung)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt im Application Core (<see cref="eBRestarter.Core.Application.Services.RestarterCycleService"/>, Use Cases, Handlers) sowie in ViewModels.<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Infrastructure.Adapters.Browsers.BrowserBase"/> und konkrete Browser-Adapter im Infrastructure Layer via OS-Prozessaufrufe).<br/>
/// - <strong>Begründung:</strong> Kapselt die physische Interaktion mit externen Webbrowser-Anwendungen (Chrome, Firefox, Edge etc.) für den Core und ist somit nach Abschnitt 1 des Leitfadens ein vorbildlicher <strong>Outbound Port</strong>.<br/>
/// - <em>Architektur-Hinweis:</em> Suffix <c>OutboundPort</c> ist perfekt.
/// </para>
/// </summary>
public interface IBrowserOutboundPort
{
    string DisplayName { get; }
    string IconPath { get; }
    string DownloadUrl { get; }

    BrowserType Type { get; }
    string ProcessName { get; }
    string BrowserVersion { get; }
    bool IsInstalled { get; }
    string ExtensionInstallUrl { get; }

    void Start(string url, string arguments = "");
    void Close();

    /// <summary>
    /// Resolves cache and path information.
    /// </summary>
    BrowserPaths ResolvePaths();

    bool IsExtensionInstalled(string? extensionId = null);
}


