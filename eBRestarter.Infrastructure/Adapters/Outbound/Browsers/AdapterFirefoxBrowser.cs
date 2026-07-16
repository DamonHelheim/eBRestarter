using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Infrastructure.Common.Statics;
using Microsoft.Extensions.Logging;

namespace eBRestarter.Infrastructure.Adapters.Outbound.Browsers;

/// <summary>
/// Adapter: Driven Adapter (Outbound) implementing Gecko/Firefox-specific control, profile management, and extension deployment.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Baustein im äußeren Ring (Infrastructure Layer) Vorgaben aus dem Core zur Prozess-, Datei- und Profilsteuerung von Mozilla Firefox.<br/>
/// - <strong>Implementierte Basis / Port:</strong> Erbt von <see cref="AdapterBrowserBase"/> (welcher <see cref="IBrowserOutboundPort"/> implementiert).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein <strong>Outbound Adapter</strong>, da sie in der Infrastrukturschicht liegt und die Steuerung einer spezifischen externen Browser-Anwendung für den Core übernimmt.
/// </para>
/// </summary>
public sealed class AdapterFirefoxBrowser(IOutboundPortOsProcessControl processControlPort, IOutboundPortSystemConfigurationRepository settingsPort, IOutboundPortFileSystem fileSystemPort, ILogger<AdapterFirefoxBrowser> logger) : AdapterBrowserBase(processControlPort, settingsPort, fileSystemPort, logger)
{
    private const string EbesucherAddOnNameForFirefox = "{76e6445a-74a5-4c26-9afc-95dae514cb77}.xpi";

    public override string DisplayName => "Firefox";
    public override string IconPath => "ms-appx:///Resources/Visuals/Icons/Intersection/fa_firefox.png";
    public override string DownloadUrl => WebLinks.FirefoxDownloadLink;
    public override string ExtensionInstallUrl => WebLinks.FirefoxEVisitorAddOnLink;
    public override BrowserType Type => BrowserType.Firefox;

    protected override List<string> ExecutablePaths
    {
        get
        {
            var paths = new List<string>();

            // 1. REGISTRY: Dynamic Lookup
            var hkcuPath = RetrievePathFromMozillaRegistry(false);

            if (!string.IsNullOrEmpty(hkcuPath))
            {
                paths.Add(hkcuPath);
            }

            var hklmPath = RetrievePathFromMozillaRegistry(true);

            if (!string.IsNullOrEmpty(hklmPath))
            {
                paths.Add(hklmPath);
            }

            // 2. DEFAULT PATHS (Fallback)
            paths.Add(_fileSystemPort.CombinePaths(_fileSystemPort.ResolveEnvironmentPath("ProgramFiles"), @"Mozilla Firefox\firefox.exe"));
            paths.Add(_fileSystemPort.CombinePaths(_fileSystemPort.ResolveEnvironmentPath("ProgramFiles(x86)"), @"Mozilla Firefox\firefox.exe"));
            paths.Add(_fileSystemPort.CombinePaths(_fileSystemPort.ResolveEnvironmentPath("LocalAppData"), @"Mozilla Firefox\firefox.exe"));

            return [.. paths.Distinct()];

            string? RetrievePathFromMozillaRegistry(bool isHklm)
            {
                const string rootKey = @"Software\Mozilla\Mozilla Firefox";

                var currentVersionObj = isHklm
                    ? _settingsPort.GetSystemValue(rootKey, "CurrentVersion")
                    : _settingsPort.GetUserValue(rootKey, "CurrentVersion");

                if (currentVersionObj is null) return null;

                string currentVersion = currentVersionObj.ToString()!;
                string mainKeyPath = $@"{rootKey}\{currentVersion}\Main";

                var pathToExeObj = isHklm
                    ? _settingsPort.GetSystemValue(mainKeyPath, "PathToExe")
                    : _settingsPort.GetUserValue(mainKeyPath, "PathToExe");

                return pathToExeObj?.ToString();
            }
        }
    }

    public override string ProcessName => "firefox";
    protected override string RegistryKeyVersion => @"Software\Mozilla\Mozilla Firefox";

    // PATHS: Resolve Cache, Cookies & Extensions for ALL profiles
    public override BrowserPaths ResolvePaths()
    {
        var appData = _fileSystemPort.ResolveEnvironmentPath("AppData"); // Roaming
        var localAppData = _fileSystemPort.ResolveEnvironmentPath("LocalAppData"); // Local

        var firefoxRoamingRoot = _fileSystemPort.CombinePaths(appData, "Mozilla", "Firefox");
        var firefoxLocalRoot = _fileSystemPort.CombinePaths(localAppData, "Mozilla", "Firefox");

        // Lists for the results
        var cacheDirs = new List<string>();
        var cookiesDirs = new List<string>();
        var extensionsDirs = new List<string>();

        // 1. Read profiles.ini and find all profile directories
        var profilesIniPath = _fileSystemPort.CombinePaths(firefoxRoamingRoot, "profiles.ini");
        var profileInfos = RetrieveProfileFoldersFromIni(profilesIniPath);

        // 2. Generate paths for each resolved profile
        foreach (var profile in profileInfos)
        {
            string fullProfilePathRoaming;
            string fullProfilePathLocal;

            if (profile.IsRelative)
            {
                // Standard case: Path is relative to AppData/Mozilla/Firefox (e.g., "Profiles/abc.default")
                fullProfilePathRoaming = _fileSystemPort.CombinePaths(firefoxRoamingRoot, profile.Path);
                fullProfilePathLocal = _fileSystemPort.CombinePaths(firefoxLocalRoot, profile.Path);
            }
            else
            {
                // Rare case: User saved the profile in a completely custom location (absolute path)
                fullProfilePathRoaming = profile.Path;

                // For absolute paths, the local cache path is harder to guess because
                // Firefox often uses a hash of the path inside LocalAppData.
                // We try a best-guess effort here: LocalRoot + directory name of the absolute path
                var folderName = new DirectoryInfo(profile.Path).Name;
                fullProfilePathLocal = _fileSystemPort.CombinePaths(firefoxLocalRoot, "Profiles", folderName);
            }

            // Path: ...\Profiles\xxxx.default\cache2\entries
            cacheDirs.Add(_fileSystemPort.CombinePaths(fullProfilePathLocal, "cache2", "entries"));

            // Optional: Also clear the startupCache
            cacheDirs.Add(_fileSystemPort.CombinePaths(fullProfilePathLocal, "startupCache"));

            // Path: ...\Profiles\xxxx.default\storage\default
            // (Firefox stores website data here inside subdirectories)
            cookiesDirs.Add(_fileSystemPort.CombinePaths(fullProfilePathRoaming, "storage", "default"));

            // Path: ...\Profiles\xxxx.default\extensions
            extensionsDirs.Add(_fileSystemPort.CombinePaths(fullProfilePathRoaming, "extensions"));
        }

        return new BrowserPaths(cacheDirs, cookiesDirs, extensionsDirs);
    }

    // CHECK: Is the extension installed? (Searches within ALL profiles)
    public override bool IsExtensionInstalled(string? extensionId = null)
    {
        var paths = ResolvePaths();

        // If no extension directories were resolved, cancel early
        if (paths.ExtensionsDirs is null || paths.ExtensionsDirs.Count == 0) return false;

        var idToCheck = string.IsNullOrEmpty(extensionId)
            ? EbesucherAddOnNameForFirefox.TrimStart('\\')
            : extensionId;

        // Verify every resolved profile directory
        foreach (var extensionsDir in paths.ExtensionsDirs)
        {
            if (!_fileSystemPort.DirectoryExists(extensionsDir)) continue;

            // Case 1: The extension is an packed .xpi file
            var xpiPath = _fileSystemPort.CombinePaths(extensionsDir, idToCheck);
            if (_fileSystemPort.FileExists(xpiPath)) return true;

            // Case 2: The extension is an unpacked directory (Sideloading)
            // Remove the .xpi extension for the folder name check
            var folderName = idToCheck.Replace(".xpi", "", StringComparison.OrdinalIgnoreCase);
            var folderPath = _fileSystemPort.CombinePaths(extensionsDir, folderName);
            if (_fileSystemPort.DirectoryExists(folderPath)) return true;
        }

        return false;
    }

    // HELPER: Parse profiles.ini
    private List<ProfileInfo> RetrieveProfileFoldersFromIni(string iniPath)
    {
        var profiles = new List<ProfileInfo>();

        if (!_fileSystemPort.FileExists(iniPath)) return profiles;

        try
        {
            // We use the file system port for line-by-line reading
            var lines = _fileSystemPort.ReadAllLines(iniPath);

            // Temporary variables for the current section block
            string? currentPath = null;
            bool? currentIsRelative = null;

            foreach (var line in lines)
            {
                // A new section starts (e.g., [Profile0] or [Install...])
                if (line.Trim().StartsWith('[') && line.Trim().EndsWith(']'))
                {
                    // If data was accumulated previously, we save it now
                    if (currentPath is not null)
                    {
                        profiles.Add(new ProfileInfo
                        {
                            Path = currentPath.Replace('/', '\\'), // Convert Linux-style slashes to Windows backslashes
                            IsRelative = currentIsRelative ?? true
                        });
                    }

                    // Reset for the next block section
                    currentPath = null;
                    currentIsRelative = null;
                    continue;
                }

                // Extract data values
                if (line.StartsWith("Path=", StringComparison.OrdinalIgnoreCase))
                {
                    currentPath = line[5..].Trim();
                }
                else if (line.StartsWith("IsRelative=", StringComparison.OrdinalIgnoreCase))
                {
                    // IsRelative=1 -> true, IsRelative=0 -> false
                    currentIsRelative = line.Trim().EndsWith('1');
                }
            }

            // Do not forget the very last entry (since no trailing brackets '[' will follow)
            if (currentPath is not null)
            {
                profiles.Add(new ProfileInfo
                {
                    Path = currentPath.Replace('/', '\\'),
                    IsRelative = currentIsRelative ?? true
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while parsing profiles.ini");
        }

        return profiles;
    }

    private sealed class ProfileInfo
    {
        public string Path { get; set; } = string.Empty;
        public bool IsRelative { get; set; } = true;
    }
}

// Cookie source
// C:\Users\Workstation\AppData\Roaming\Mozilla\Firefox\Profiles\opng5oi1.default-release\storage\default => Cookies
// "C:\Users\Workstation\AppData\Roaming\Mozilla\Firefox\Profiles\opng5oi1.default-release\cookies.sqlite"

// Internet cache source
// C:\Users\Workstation\AppData\Local\Mozilla\Firefox\Profiles\opng5oi1.default-release\cache2\doomed
// C:\Users\Workstation\AppData\Local\Mozilla\Firefox\Profiles\opng5oi1.default-release\cache2\entries
// C:\Users\Workstation\AppData\Local\Mozilla\Firefox\Profiles\opng5oi1.default-release\jumpListCache

// Current open tabs
// C:\Users\Workstation\AppData\Roaming\Mozilla\Firefox\Profiles\opng5oi1.default-release\sessionstore-backups
// C:\Users\Workstation\AppData\Roaming\Mozilla\Firefox\Profiles\opng5oi1.default-release\datareporting


