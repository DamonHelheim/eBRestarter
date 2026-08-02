using Microsoft.Extensions.Logging;

using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Infrastructure.Common.Statics;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Wrapper.Browsers;

/// <summary>
/// Adapter: Driven Adapter (Outbound) implementing Chromium-based control and discovery for Microsoft Edge.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Baustein im äußeren Ring (Infrastructure Layer) Vorgaben aus dem Core zur Prozess-, Datei- und Konfigurationssteuerung von Microsoft Edge.<br/>
/// - <strong>Implementierte Basis / Port:</strong> Erbt von <see cref="AdapterChromiumBrowserBaseWrapper"/> (welcher <see cref="IBrowserOutboundPort"/> implementiert).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein <strong>Outbound Adapter</strong>, da sie in der Infrastrukturschicht liegt und die Steuerung einer spezifischen externen Browser-Anwendung für den Core übernimmt.
/// </para>
/// </summary>
public sealed class AdapterEdgeBrowserWrapper(
    IOutboundPortOsProcessControl processControlPort,
    IOutboundPortSystemConfigurationRepository settingsPort,
    IOutboundPortFileSystem fileSystemPort,
    ILogger<AdapterEdgeBrowserWrapper> logger)
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
    private const string DefaultBrowserRegistryName = "Microsoft Edge";
    private const string DefaultDisplayName = "Edge";
    private const string DefaultExeFileName = "msedge.exe";
    private const string DefaultExtensionId = "kjhejmaladginnedpoppohfnkionnghi";
    private const string DefaultIconPath = "ms-appx:///Resources/Visuals/Icons/Intersection/fa_edge.png";
    private const string DefaultProcessName = "msedge";
    private const string DefaultProgramFilesSubPath = @"Microsoft\Edge\Application";
    private const string DefaultRegistryKeyVersion = @"Software\Microsoft\Edge\BLBeacon";
    private const string DefaultUninstallSubKey = "Microsoft Edge";

    // ── Block 4: Komplexe Typen, Collections & UI-Elemente (alphabetisch) ──
    private static readonly string[] _userDataSubPath = ["Microsoft", "Edge", "User Data"];


    // ═══════════════════════════════════════════════════════
    //  4. Properties
    // ═══════════════════════════════════════════════════════

    // ── Block 2: Primitive Typen & Strings (alphabetisch) ──
    protected override string BrowserRegistryName => DefaultBrowserRegistryName;
    public override string DisplayName => DefaultDisplayName;
    public override string DownloadUrl => WebLinks.EdgeDownloadLinkDE;
    protected override string ExeFileName => DefaultExeFileName;
    protected override string ExtensionId => DefaultExtensionId;
    public override string ExtensionInstallUrl => WebLinks.EdgeEVisitorAddOnLink;
    public override string IconPath => DefaultIconPath;
    public override string ProcessName => DefaultProcessName;
    protected override string ProgramFilesSubPath => DefaultProgramFilesSubPath;
    protected override string RegistryKeyVersion => DefaultRegistryKeyVersion;
    protected override string UninstallSubKey => DefaultUninstallSubKey;

    // ── Block 3: Enums (alphabetisch) ──
    public override BrowserType Type => BrowserType.Edge;

    // ── Block 4: Komplexe Typen, Collections & UI-Elemente (alphabetisch) ──
    protected override string[] UserDataSubPath => _userDataSubPath;
}
