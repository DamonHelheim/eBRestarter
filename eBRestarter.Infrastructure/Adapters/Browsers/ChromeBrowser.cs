using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using eBRestarter.Infrastructure.Browser;
using Microsoft.Extensions.Logging;

namespace eBRestarter.Infrastructure.Adapters.Browsers;

public sealed class ChromeBrowser(IOsProcessControlPort processControlPort, ISettingsPort settingsPort, IFileSystemPort fileSystemPort, ILogger<ChromeBrowser> logger) : ChromiumBrowserBase(processControlPort, settingsPort, fileSystemPort, logger)
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


