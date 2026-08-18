using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.ObjectArchetypes.Enums;

namespace eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;

/// <summary>
/// Port: Driven Port (Outbound) defining the capabilities for inspecting, launching, controlling, and terminating concrete OS Web Browsers.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND PORT (Driven Port / Steckdose für Browser-Steuerung)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt im Application Core (<see cref="Services.RestarterCycleService"/>, Use Cases, Handlers) sowie in ViewModels.<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Infrastructure.Adapters.Browsers.BrowserBase"/> und konkrete Browser-Adapter im Infrastructure Layer via OS-Prozessaufrufe).<br/>
/// - <strong>Begründung:</strong> Kapselt die physische Interaktion mit externen Webbrowser-Anwendungen (Chrome, Firefox, Edge etc.) für den Core und ist somit nach Abschnitt 1 des Leitfadens ein vorbildlicher <strong>Outbound Port</strong>.<br/>
/// - <em>Architektur-Hinweis:</em> Suffix <c>OutboundPort</c> ist perfekt.
/// </para>
/// </summary>
public interface IOutboundPortBrowser
{
    string DisplayName { get; }
    string IconPath { get; }
    string DownloadUrl { get; }

    BrowserType Type { get; }
    string ProcessName { get; }
    string BrowserVersion { get; }
    bool IsInstalled { get; }
    string ExtensionInstallUrl { get; }

    /// <summary>
    /// Launches the browser with the given URL.
    /// </summary>
    /// <param name="url">Absolute URL to open. Must not be <see langword="null"/> or whitespace.</param>
    /// <param name="arguments">Optional additional command-line arguments.</param>
    /// <returns>
    /// <see langword="true"/> if the browser process was started; <see langword="false"/> if it
    /// could not be started — most commonly because the browser is not installed.
    /// </returns>
    /// <remarks>
    /// ⚠️ Exception-Guideline Kap. 2: Der Rückgabewert existiert, weil "Browser nicht installiert"
    /// in dieser Anwendung ein <b>erwarteter</b> Zustand ist, kein Ausnahmefall — die gesamte
    /// Browser-Auswahl der Oberfläche dreht sich darum. Vorher gab die Methode <c>void</c> zurück
    /// und verschluckte jeden Fehler intern: der Aufrufer konnte einen fehlgeschlagenen Start
    /// nicht von einem erfolgreichen unterscheiden und lief mit einer falschen Annahme weiter.
    /// </remarks>
    bool Start(string url, string arguments = "");

    void Close();

    /// <summary>
    /// Resolves cache and path information.
    /// </summary>
    BrowserPaths ResolvePaths();

    bool IsExtensionInstalled(string? extensionId = null);
}


