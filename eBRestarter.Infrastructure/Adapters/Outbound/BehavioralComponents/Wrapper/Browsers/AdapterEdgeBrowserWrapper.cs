using Microsoft.Extensions.Logging;

using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Infrastructure.Common.Statics;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Wrapper.Browsers;

/// <summary>
/// Adapter: Driven Adapter (Outbound) implementing Chromium-based control and discovery for Microsoft Edge.
/// <para>
/// <strong>Architecture Classification: OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Role &amp; Responsibility:</strong> Handles process, file, and configuration control for Microsoft Edge in the Infrastructure layer.<br/>
/// - <strong>Implemented Base / Port:</strong> Inherits from <see cref="AdapterChromiumBrowserBaseWrapper"/> (which implements <see cref="IOutboundPortBrowser"/>).<br/>
/// </para>
/// </summary>
/// <param name="processControlPort">OS process control port.</param>
/// <param name="settingsPort">System configuration repository port.</param>
/// <param name="fileSystemPort">File system operations port.</param>
/// <param name="logger">Logger instance.</param>
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

    // ── Block 2: Primitives & strings ──
    private const string DefaultBrowserRegistryName = "Microsoft Edge";
    private const string DefaultDisplayName = "Edge";
    private const string DefaultExeFileName = "msedge.exe";
    private const string DefaultExtensionId = "kjhejmaladginnedpoppohfnkionnghi";
    private const string DefaultIconPath = "ms-appx:///Resources/Visuals/Icons/Intersection/fa_edge.png";
    private const string DefaultProcessName = "msedge";
    private const string DefaultProgramFilesSubPath = @"Microsoft\Edge\Application";
    private const string DefaultRegistryKeyVersion = @"Software\Microsoft\Edge\BLBeacon";
    private const string DefaultUninstallSubKey = "Microsoft Edge";

    // ── Block 4: Complex types & collections ──
    private static readonly string[] _userDataSubPath = ["Microsoft", "Edge", "User Data"];


    // ═══════════════════════════════════════════════════════
    //  4. Properties
    // ═══════════════════════════════════════════════════════

    // ── Block 2: Primitives & strings ──
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

    // ── Block 3: Enums ──
    public override BrowserType Type => BrowserType.Edge;

    // ── Block 4: Complex types & collections ──
    protected override string[] UserDataSubPath => _userDataSubPath;
}
