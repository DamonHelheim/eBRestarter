using Microsoft.Extensions.Logging;

using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Infrastructure.Common.Statics;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Wrapper.Browsers;

/// <summary>
/// Adapter: Driven Adapter (Outbound) implementing Chromium-based control and discovery for Vivaldi Browser.
/// <para>
/// <strong>Architecture Classification: OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Role &amp; Responsibility:</strong> Handles process, file, and configuration control for Vivaldi Browser in the Infrastructure layer.<br/>
/// - <strong>Implemented Base / Port:</strong> Inherits from <see cref="AdapterChromiumBrowserBaseWrapper"/> (which implements <see cref="IOutboundPortBrowser"/>).<br/>
/// </para>
/// </summary>
/// <param name="processControlPort">OS process control port.</param>
/// <param name="settingsPort">System configuration repository port.</param>
/// <param name="fileSystemPort">File system operations port.</param>
/// <param name="logger">Logger instance.</param>
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

    // ── Block 2: Primitives & strings ──
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

    // ── Block 4: Complex types & collections ──
    private static readonly string[] _userDataSubPath = ["Vivaldi", "User Data"];


    // ═══════════════════════════════════════════════════════
    //  4. Properties
    // ═══════════════════════════════════════════════════════

    // ── Block 2: Primitives & strings ──
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

    // ── Block 3: Enums ──
    public override BrowserType Type => BrowserType.Vivaldi;

    // ── Block 4: Complex types & collections ──
    protected override string[] UserDataSubPath => _userDataSubPath;
}
