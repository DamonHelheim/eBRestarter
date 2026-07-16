using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Infrastructure.Common.Statics;
using Microsoft.Extensions.Logging;

namespace eBRestarter.Infrastructure.Adapters.Outbound.Browsers;

/// <summary>
/// Adapter: Driven Adapter (Outbound) implementing Chromium-based control and discovery for Brave Browser.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Baustein im äußeren Ring (Infrastructure Layer) Vorgaben aus dem Core zur Prozess-, Datei- und Konfigurationssteuerung von Brave.<br/>
/// - <strong>Implementierte Basis / Port:</strong> Erbt von <see cref="AdapterChromiumBrowserBase"/> (welcher <see cref="IOutboundPortBrowser"/> implementiert).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein <strong>Outbound Adapter</strong>, da sie in der Infrastrukturschicht liegt und die Steuerung einer spezifischen externen Browser-Anwendung für den Core übernimmt.
/// </para>
/// </summary>
public sealed class AdapterBraveBrowser(IOutboundPortOsProcessControl processControlPort, IOutboundPortSystemConfigurationRepository settingsPort, IOutboundPortFileSystem fileSystemPort, ILogger<AdapterBraveBrowser> logger) : AdapterChromiumBrowserBase(processControlPort, settingsPort, fileSystemPort, logger)
{
    public override string DisplayName => "Brave";
    public override string IconPath => "ms-appx:///Resources/Visuals/Icons/Intersection/fa_brave.png";
    public override string DownloadUrl => WebLinks.BraveDownloadLinkDE;

    public override BrowserType Type => BrowserType.Brave;

    public override string ExtensionInstallUrl => "https://chrome.google.com/webstore/detail/ebesucher-addon/agchmcconfdfcenopioeilpgjngelefk";

    protected override string ExeFileName => "brave.exe";
    protected override string BrowserRegistryName => "Brave"; // Or "BraveSoftware Brave-Browser", depending on the registry
    protected override string UninstallSubKey => "BraveSoftware Brave-Browser";
    protected override string ProgramFilesSubPath => @"BraveSoftware\Brave-Browser\Application";

    // Brave uses "brave" as its process name
    public override string ProcessName => "brave";

    // Registry key for version checking
    protected override string RegistryKeyVersion => @"Software\BraveSoftware\Brave-Browser\BLBeacon";
    protected override string ExtensionId => "agchmcconfdfcenopioeilpgjngelefk";
    protected override string[] UserDataSubPath => ["BraveSoftware", "Brave-Browser", "User Data"];
}


