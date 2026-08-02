using Microsoft.Extensions.Logging;

using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Infrastructure.Common.Statics;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Wrapper.Browsers;

/// <summary>
/// Adapter: Driven Adapter (Outbound) implementing Chromium-based control and discovery for Google Chrome.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Baustein im äußeren Ring (Infrastructure Layer) Vorgaben aus dem Core zur Prozess-, Datei- und Konfigurationssteuerung von Google Chrome.<br/>
/// - <strong>Implementierte Basis / Port:</strong> Erbt von <see cref="AdapterChromiumBrowserBaseWrapper"/> (welcher <see cref="IBrowserOutboundPort"/> implementiert).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein <strong>Outbound Adapter</strong>, da sie in der Infrastrukturschicht liegt und die Steuerung einer spezifischen externen Browser-Anwendung für den Core übernimmt.
/// </para>
/// </summary>
public sealed class AdapterChromeBrowserWrapper(
    IOutboundPortOsProcessControl processControlPort,
    IOutboundPortSystemConfigurationRepository settingsPort,
    IOutboundPortFileSystem fileSystemPort,
    ILogger<AdapterChromeBrowserWrapper> logger)
    : AdapterChromiumBrowserBaseWrapper(
        processControlPort,
        settingsPort,
        fileSystemPort,
        logger)
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════

    // ── Block 2: Primitive Typen & Strings (alphabetisch) ──
    private const string DefaultBrowserRegistryName = "Google Chrome";
    private const string DefaultDisplayName = "Chrome";
    private const string DefaultExeFileName = "chrome.exe";
    private const string DefaultExtensionId = "agchmcconfdfcenopioeilpgjngelefk";
    private const string DefaultIconPath = "ms-appx:///Resources/Visuals/Icons/Intersection/fa_chrome.png";
    private const string DefaultProcessName = "chrome";
    private const string DefaultProgramFilesSubPath = @"Google\Chrome\Application";
    private const string DefaultRegistryKeyVersion = @"Software\Google\Chrome\BLBeacon";
    private const string DefaultUninstallSubKey = "Google Chrome";

    // ── Block 4: Komplexe Typen, Collections & UI-Elemente (alphabetisch) ──
    private static readonly string[] _userDataSubPath = ["Google", "Chrome", "User Data"];


    // ═══════════════════════════════════════════════════════
    //  4. Properties
    // ═══════════════════════════════════════════════════════

    // ── Block 2: Primitive Typen & Strings (alphabetisch) ──
    protected override string BrowserRegistryName => DefaultBrowserRegistryName;
    public override string DisplayName => DefaultDisplayName;
    public override string DownloadUrl => WebLinks.ChromeDownloadLinkDE;
    protected override string ExeFileName => DefaultExeFileName;
    protected override string ExtensionId => DefaultExtensionId;
    public override string ExtensionInstallUrl => WebLinks.ChromeEVisitorAddOnLink;
    public override string IconPath => DefaultIconPath;
    public override string ProcessName => DefaultProcessName;
    protected override string ProgramFilesSubPath => DefaultProgramFilesSubPath;
    protected override string RegistryKeyVersion => DefaultRegistryKeyVersion;
    protected override string UninstallSubKey => DefaultUninstallSubKey;

    // ── Block 3: Enums (alphabetisch) ──
    public override BrowserType Type => BrowserType.Chrome;

    // ── Block 4: Komplexe Typen, Collections & UI-Elemente (alphabetisch) ──
    protected override string[] UserDataSubPath => _userDataSubPath;
}
