using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Infrastructure.Adapters.Outbound.Browsers;
using eBRestarter.Infrastructure.Common.Statics;
using Microsoft.Extensions.Logging;

namespace eBRestarter.Infrastructure.Adapters.Browsers;

/// <summary>
/// Adapter: Driven Adapter (Outbound) implementing Chromium-based control and discovery for Microsoft Edge.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Baustein im äußeren Ring (Infrastructure Layer) Vorgaben aus dem Core zur Prozess-, Datei- und Konfigurationssteuerung von Microsoft Edge.<br/>
/// - <strong>Implementierte Basis / Port:</strong> Erbt von <see cref="AdapterChromiumBrowserBase"/> (welcher <see cref="IBrowserOutboundPort"/> implementiert).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein <strong>Outbound Adapter</strong>, da sie in der Infrastrukturschicht liegt und die Steuerung einer spezifischen externen Browser-Anwendung für den Core übernimmt.
/// </para>
/// </summary>
public sealed class AdapterEdgeBrowser(IOutboundPortOsProcessControl processControlPort, IOutboundPortSystemConfigurationRepository settingsPort, IOutboundPortFileSystem fileSystemPort, ILogger<AdapterEdgeBrowser> logger) : AdapterChromiumBrowserBase(processControlPort, settingsPort, fileSystemPort, logger)
{
    public override string DisplayName => "Edge";
    public override string IconPath => "ms-appx:///Resources/Visuals/Icons/Intersection/fa_edge.png";
    public override string DownloadUrl => WebLinks.EdgeDownloadLinkDE;
    public override string ExtensionInstallUrl => WebLinks.EdgeEVisitorAddOnLink;

    public override BrowserType Type => BrowserType.Edge;

    // Edge uses "msedge" as its process name
    public override string ProcessName => "msedge";

    // Registry key for version checking (similar to Chrome)
    protected override string RegistryKeyVersion => @"Software\Microsoft\Edge\BLBeacon";
    protected override string ExtensionId => "kjhejmaladginnedpoppohfnkionnghi";
    protected override string[] UserDataSubPath => ["Microsoft", "Edge", "User Data"];

    protected override string ExeFileName => "msedge.exe";

    // HKLM\SOFTWARE\Clients\StartMenuInternet\Microsoft Edge
    protected override string BrowserRegistryName => "Microsoft Edge";

    // HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Microsoft Edge
    protected override string UninstallSubKey => "Microsoft Edge";

    // C:\Program Files (x86)\Microsoft\Edge\Application
    protected override string ProgramFilesSubPath => @"Microsoft\Edge\Application";
}


