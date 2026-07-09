using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Infrastructure.Common.Statics;
using Microsoft.Extensions.Logging;

namespace eBRestarter.Infrastructure.Adapters.Outbound.Browsers;

/// <summary>
/// Adapter: Driven Adapter (Outbound) implementing Chromium-based control and discovery for Google Chrome.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Baustein im äußeren Ring (Infrastructure Layer) Vorgaben aus dem Core zur Prozess-, Datei- und Konfigurationssteuerung von Google Chrome.<br/>
/// - <strong>Implementierte Basis / Port:</strong> Erbt von <see cref="AdapterChromiumBrowserBase"/> (welcher <see cref="IBrowserOutboundPort"/> implementiert).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein <strong>Outbound Adapter</strong>, da sie in der Infrastrukturschicht liegt und die Steuerung einer spezifischen externen Browser-Anwendung für den Core übernimmt.
/// </para>
/// </summary>
public sealed class AdapterChromeBrowser(IOutboundPortOsProcessControl processControlPort, IOutboundPortSystemConfigurationRepository settingsPort, IOutboundPortFileSystem fileSystemPort, ILogger<AdapterChromeBrowser> logger) : AdapterChromiumBrowserBase(processControlPort, settingsPort, fileSystemPort, logger)
{
    public override BrowserType Type => BrowserType.Chrome;
    public override string DisplayName => "Chrome";
    public override string IconPath => "ms-appx:///Resources/Visuals/Icons/Intersection/fa_chrome.png";
    public override string DownloadUrl => WebLinks.ChromeDownloadLinkDE;
    public override string ExtensionInstallUrl => WebLinks.ChromeEVisitorAddOnLink;

    // Implementation of abstract properties for the search strategy
    protected override string ExeFileName => "chrome.exe";
    protected override string BrowserRegistryName => "Google Chrome";
    protected override string UninstallSubKey => "Google Chrome";
    protected override string ProgramFilesSubPath => @"Google\Chrome\Application";

    public override string ProcessName => "chrome";

    // Chrome often stores its version under HKCU\BLBeacon
    protected override string RegistryKeyVersion => @"Software\Google\Chrome\BLBeacon";
    protected override string ExtensionId => "agchmcconfdfcenopioeilpgjngelefk";
    protected override string[] UserDataSubPath => ["Google", "Chrome", "User Data"];
}


