using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using Microsoft.Extensions.Logging;

namespace eBRestarter.Infrastructure.Adapters.Browsers;

public abstract class ChromiumBrowserBase(IOsProcessControlOutboundPort processControlPort, ISettingsRepositoryOutboundPort settingsPort, IFileSystemOutboundPort fileSystemPort, ILogger logger) : BrowserBase(processControlPort, settingsPort, fileSystemPort, logger)
{
    // These values must be provided by the concrete implementation classes
    protected abstract string ExeFileName { get; }          // e.g., "chrome.exe"
    protected abstract string BrowserRegistryName { get; }  // e.g., "Google Chrome"
    protected abstract string UninstallSubKey { get; }      // e.g., "Google Chrome" or "BraveSoftware Brave-Browser"
    protected abstract string ProgramFilesSubPath { get; }  // e.g., @"Google\Chrome\Application"

    // Must be implemented by Chrome/Edge/Brave
    protected abstract string ExtensionId { get; }

    // Returns the path components leading to the User Data folder (e.g., ["Google", "Chrome", "User Data"])
    protected abstract string[] UserDataSubPath { get; }

    protected override List<string> ExecutablePaths
    {
        get
        {
            var paths = new List<string>();

            // 1. App Paths
            var appPathKey = $@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\{ExeFileName}";
            var appPath = _settingsPort.GetSystemValue(appPathKey, "")?.ToString();

            if (!string.IsNullOrEmpty(appPath)) { paths.Add(appPath); }

            // 2. StartMenuInternet
            var clientKey = $@"SOFTWARE\Clients\StartMenuInternet\{BrowserRegistryName}\shell\open\command";
            var clientPath = _settingsPort.GetSystemValue(clientKey, "")?.ToString();

            if (!string.IsNullOrEmpty(clientPath)) { paths.Add(clientPath.Replace("\"", "").Trim()); }

            // 3. Uninstall Keys (Using our generic helper logic here)
            AddPathFromUninstallKey(paths, $@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{UninstallSubKey}", ExeFileName, true);
            AddPathFromUninstallKey(paths, $@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{UninstallSubKey}", ExeFileName, true);
            AddPathFromUninstallKey(paths, $@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{UninstallSubKey}", ExeFileName, false);

            // 4. Standard default paths
            var pathPart = _fileSystemPort.CombinePaths(ProgramFilesSubPath, ExeFileName);

            paths.Add(_fileSystemPort.CombinePaths(_fileSystemPort.ResolveEnvironmentPath("ProgramFiles"), pathPart));
            paths.Add(_fileSystemPort.CombinePaths(_fileSystemPort.ResolveEnvironmentPath("ProgramFiles(x86)"), pathPart));
            paths.Add(_fileSystemPort.CombinePaths(_fileSystemPort.ResolveEnvironmentPath("LocalAppData"), pathPart));

            return [.. paths.Distinct()];
        }
    }

    public override bool IsExtensionInstalled(string? extensionId = null)
    {
        // If no specific ID was provided, fall back to the browser's default extension ID
        var idToCheck = string.IsNullOrEmpty(extensionId) ? ExtensionId : extensionId;

        if (string.IsNullOrEmpty(idToCheck)) return false;

        var paths = ResolvePaths();

        // If no extension directories were resolved (e.g., no profiles found), nothing is installed
        if (paths.ExtensionsDirs == null || paths.ExtensionsDirs.Count == 0) return false;

        // Verify EVERY resolved profile extension directory
        foreach (var extensionsDir in paths.ExtensionsDirs)
        {
            if (!_fileSystemPort.DirectoryExists(extensionsDir)) continue;

            // Inconsistency Mitigation Logic:
            // Some subclasses (e.g., Brave) return the full path INCLUDING the ID.
            // Others (e.g., Chrome/Edge, when corrected) only return the parent ".../Extensions" folder.

            // Case A: The path already ends with the extension ID (Brave style)
            if (extensionsDir.EndsWith(idToCheck, StringComparison.OrdinalIgnoreCase))
            {
                // The folder already exists (verified above), so the extension is present.
                // (Optionally, one could verify if version subdirectories exist inside)
                return true;
            }

            // Case B: The path is just the base "Extensions" directory -> We must append the ID
            var fullExtensionPath = _fileSystemPort.CombinePaths(extensionsDir, idToCheck);

            if (_fileSystemPort.DirectoryExists(fullExtensionPath))
            {
                return true;
            }
        }

        return false;
    }

    public override BrowserPaths ResolvePaths()
    {
        var localAppData = _fileSystemPort.ResolveEnvironmentPath("LocalAppData");

        var pathParts = new List<string> { localAppData };
        pathParts.AddRange(UserDataSubPath);
        var userDataRoot = _fileSystemPort.CombinePaths([.. pathParts]);

        var cacheDirs = new List<string>();
        var cookiesDirs = new List<string>();
        var extensionsDirs = new List<string>();

        var allProfileFolders = new List<string>();

        var defaultPath = _fileSystemPort.CombinePaths(userDataRoot, "Default");

        if (_fileSystemPort.DirectoryExists(defaultPath))
        {
            allProfileFolders.Add(defaultPath);
        }

        try
        {
            var dirs = _fileSystemPort.GetDirectories(userDataRoot, "Profile *");
            allProfileFolders.AddRange(dirs);
        }
        catch { /* Error handling if directory does not exist */ }

        foreach (var profilePath in allProfileFolders)
        {
            cacheDirs.Add(_fileSystemPort.CombinePaths(profilePath, "Cache", "Cache_Data"));
            cacheDirs.Add(_fileSystemPort.CombinePaths(profilePath, "Service Worker"));
            cacheDirs.Add(_fileSystemPort.CombinePaths(profilePath, "Code Cache"));
            cacheDirs.Add(_fileSystemPort.CombinePaths(profilePath, "GPUCache"));

            cookiesDirs.Add(_fileSystemPort.CombinePaths(profilePath, "IndexedDB"));
            cookiesDirs.Add(_fileSystemPort.CombinePaths(profilePath, "Network"));
            cookiesDirs.Add(_fileSystemPort.CombinePaths(profilePath, "Local Storage"));
            cookiesDirs.Add(_fileSystemPort.CombinePaths(profilePath, "Session Storage"));

            extensionsDirs.Add(_fileSystemPort.CombinePaths(profilePath, "Extensions", ExtensionId));
        }

        return new BrowserPaths(cacheDirs, cookiesDirs, extensionsDirs);
    }
}

