using Microsoft.Extensions.Logging;

using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Infrastructure.Common.Statics;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Wrapper.Browsers;

/// <summary>
/// Adapter: Driven Adapter (Outbound) implementing Chromium-based control and discovery for Vivaldi Browser.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Baustein im äußeren Ring (Infrastructure Layer) Vorgaben aus dem Core zur Prozess-, Datei- und Konfigurationssteuerung von Vivaldi.<br/>
/// - <strong>Implementierte Basis / Port:</strong> Erbt von <see cref="AdapterChromiumBrowserBaseWrapper"/> (welcher <see cref="IOutboundPortBrowser"/> implementiert).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein <strong>Outbound Adapter</strong>, da sie in der Infrastrukturschicht liegt und die Steuerung einer spezifischen externen Browser-Anwendung für den Core übernimmt.
/// </para>
/// </summary>
public sealed class AdapterVivaldiBrowserWrapper(
    IOutboundPortOsProcessControl processControlPort,
    IOutboundPortSystemConfigurationRepository settingsPort,
    IOutboundPortFileSystem fileSystemPort,
    ILogger<AdapterVivaldiBrowserWrapper> logger)
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
    private const string DefaultBrowserRegistryName = "Vivaldi";
    private const string DefaultDisplayName = "Vivaldi";
    private const string DefaultExeFileName = "vivaldi.exe";
    private const string DefaultExtensionId = "agchmcconfdfcenopioeilpgjngelefk";
    private const string DefaultIconPath = "ms-appx:///Resources/Visuals/Icons/Intersection/icons8_vivaldi.png";
    private const string DefaultProcessName = "vivaldi";
    private const string DefaultProgramFilesSubPath = @"Vivaldi\Application";
    private const string DefaultRegistryKeyVersion = @"Software\Vivaldi\BLBeacon";
    private const string DefaultUninstallSubKey = "Vivaldi";
    private const string DisplayVersionValueName = "DisplayVersion";
    private const string MachineUninstallRegistryKeyPattern = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{0}";
    private const string Wow64UninstallRegistryKeyPattern = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{0}";

    // ── Block 4: Komplexe Typen, Collections & UI-Elemente (alphabetisch) ──
    private static readonly string[] _userDataSubPath = ["Vivaldi", "User Data"];


    // ═══════════════════════════════════════════════════════
    //  4. Properties
    // ═══════════════════════════════════════════════════════

    // ── Block 2: Primitive Typen & Strings (alphabetisch) ──
    protected override string BrowserRegistryName => DefaultBrowserRegistryName;

    public override string BrowserVersion
    {
        get
        {
            var uninstallPath = string.Format(MachineUninstallRegistryKeyPattern, UninstallSubKey);
            var uninstallPathWow = string.Format(Wow64UninstallRegistryKeyPattern, UninstallSubKey);

            var versionObj = _settingsPort.GetUserValue(uninstallPath, DisplayVersionValueName)
                ?? _settingsPort.GetSystemValue(uninstallPath, DisplayVersionValueName)
                ?? _settingsPort.GetSystemValue(uninstallPathWow, DisplayVersionValueName);

            if (versionObj?.ToString() is { Length: > 0 } versionString)
            {
                return CleanVersionString(versionString);
            }

            return base.BrowserVersion;
        }
    }

    public override string DisplayName => DefaultDisplayName;
    public override string DownloadUrl => WebLinks.VivaldiDownloadLinkDE;
    protected override string ExeFileName => DefaultExeFileName;
    protected override string ExtensionId => DefaultExtensionId;
    public override string ExtensionInstallUrl => WebLinks.ChromeEVisitorAddOnLink;
    public override string IconPath => DefaultIconPath;
    public override string ProcessName => DefaultProcessName;
    protected override string ProgramFilesSubPath => DefaultProgramFilesSubPath;
    protected override string RegistryKeyVersion => DefaultRegistryKeyVersion;
    protected override string UninstallSubKey => DefaultUninstallSubKey;

    // ── Block 3: Enums (alphabetisch) ──
    public override BrowserType Type => BrowserType.Vivaldi;

    // ── Block 4: Komplexe Typen, Collections & UI-Elemente (alphabetisch) ──
    protected override string[] UserDataSubPath => _userDataSubPath;
}
