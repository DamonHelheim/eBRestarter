using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using eBRestarter.Infrastructure.Browser;
using Microsoft.Extensions.Logging;

namespace eBRestarter.Infrastructure.Adapters.Browsers;

public sealed class VivaldiBrowser(IOsProcessControlPort processControlPort, ISettingsPort settingsPort, IFileSystemPort fileSystemPort, ILogger<VivaldiBrowser> logger) : ChromiumBrowserBase(processControlPort, settingsPort, fileSystemPort, logger)
{
    public override BrowserType Type => BrowserType.Vivaldi;
    public override string DisplayName => "Vivaldi";
    public override string IconPath => "ms-appx:///Resources/Visuals/Icons/Intersection/icons8_vivaldi.png"; // Please ensure this icon exists

    // Assumption: You will add this link to your WebLinks constants
    public override string DownloadUrl => WebLinks.VivaldiDownloadLinkDE;

    public override string ExtensionInstallUrl => WebLinks.ChromeEVisitorAddOnLink;

    public override string BrowserVersion
    {
        get
        {
            // 1. Define paths to the uninstall key
            string uninstallPathCU = $@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{UninstallSubKey}";
            string uninstallPathLM = $@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{UninstallSubKey}";
            string uninstallPathWow = $@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{UninstallSubKey}";

            // 2. Search in Current User (HKCU) first (Default for Vivaldi)
            var version = _settingsPort.GetUserValue(uninstallPathCU, "DisplayVersion");

            // 3. If not found, look inside Local Machine (HKLM)
            version ??= _settingsPort.GetSystemValue(uninstallPathLM, "DisplayVersion")
                ?? _settingsPort.GetSystemValue(uninstallPathWow, "DisplayVersion");

            // 4. If we found the Vivaldi version, clean it and return it
            if (version is not null && !string.IsNullOrEmpty(version.ToString()))
            {
                return CleanVersionString(version.ToString());
            }

            // 5. Fallback: If the uninstall key is missing for some reason,
            // we fall back to the default Chromium logic from the base class (BLBeacon).
            return base.BrowserVersion;
        }
    }

    // Implementation of abstract properties for the search strategy
    protected override string ExeFileName => "vivaldi.exe";
    protected override string BrowserRegistryName => "Vivaldi";
    protected override string UninstallSubKey => "Vivaldi";
    protected override string ProgramFilesSubPath => @"Vivaldi\Application";

    public override string ProcessName => "vivaldi";

    // Vivaldi often stores its version in the AutoUpdate key or the Uninstall key.
    // If "BLBeacon" does not work for Vivaldi, you might want to check @"Software\Vivaldi" here instead.
    protected override string RegistryKeyVersion => @"Software\Vivaldi\BLBeacon";

    // Vivaldi directly supports Chrome extensions from the Chrome Web Store
    protected override string ExtensionId => "agchmcconfdfcenopioeilpgjngelefk";
    protected override string[] UserDataSubPath => ["Vivaldi", "User Data"];
}


