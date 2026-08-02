using Microsoft.Extensions.Logging;

using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Wrapper.Browsers;

/// <summary>
/// Adapter: Base Driven Adapter (Outbound) encapsulating common Chromium logic (extension deployment, registry paths, process discovery).
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Dient als technologische Zwischen-Basisklasse im äußeren Ring (Infrastructure Layer) für alle Chromium-basierten Browser.<br/>
/// - <strong>Implementierte Basis / Port:</strong> Erbt von <see cref="AdapterBrowserBaseWrapper"/> (welcher <see cref="IBrowserOutboundPort"/> implementiert).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein vorbildlicher <strong>Outbound Adapter</strong>, da sie in der Infrastrukturschicht liegt und technologische Operationen für Chromium-Browser ausführt.
/// </para>
/// </summary>
public abstract class AdapterChromiumBrowserBaseWrapper(
    IOutboundPortOsProcessControl processControlPort,
    IOutboundPortSystemConfigurationRepository settingsPort,
    IOutboundPortFileSystem fileSystemPort,
    ILogger logger)
    : AdapterBrowserBaseWrapper(
        processControlPort,
        settingsPort,
        fileSystemPort,
        logger)
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════

    // ── Block 2: Primitive Typen & Strings (alphabetisch) ──
    private const string AppPathsRegistryKeyPattern = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\{0}";
    private const string CacheDataSubPath = "Cache_Data";
    private const string CacheSubPath = "Cache";
    private const string CodeCacheSubPath = "Code Cache";
    private const string DefaultProfileFolderName = "Default";
    private const string ExtensionsFolderName = "Extensions";
    private const string GpuCacheSubPath = "GPUCache";
    private const string IndexedDbSubPath = "IndexedDB";
    private const string LocalAppDataEnvVar = "LocalAppData";
    private const string LocalStorageSubPath = "Local Storage";
    private const string MachineUninstallRegistryKeyPattern = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{0}";
    private const string NetworkSubPath = "Network";
    private const string ProfileFolderSearchPattern = "Profile *";
    private const string ProgramFilesEnvVar = "ProgramFiles";
    private const string ProgramFilesX86EnvVar = "ProgramFiles(x86)";
    private const string ServiceWorkerSubPath = "Service Worker";
    private const string SessionStorageSubPath = "Session Storage";
    private const string StartMenuInternetRegistryKeyPattern = @"SOFTWARE\Clients\StartMenuInternet\{0}\shell\open\command";
    private const string UserUninstallRegistryKeyPattern = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\{0}";
    private const string Wow64UninstallRegistryKeyPattern = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{0}";


    // ═══════════════════════════════════════════════════════
    //  4. Properties
    // ═══════════════════════════════════════════════════════

    // ── Block 2: Primitive Typen & Strings (alphabetisch) ──
    protected abstract string BrowserRegistryName { get; }
    protected abstract string ExeFileName { get; }
    protected abstract string ExtensionId { get; }
    protected abstract string ProgramFilesSubPath { get; }
    protected abstract string UninstallSubKey { get; }

    // ── Block 4: Komplexe Typen, Collections & UI-Elemente (alphabetisch) ──
    protected override List<string> ExecutablePaths
    {
        get
        {
            var paths = new List<string>();

            // 1. App Paths
            var appPathKey = string.Format(AppPathsRegistryKeyPattern, ExeFileName);
            var appPath = _settingsPort.GetSystemValue(appPathKey, string.Empty)?.ToString();

            if (!string.IsNullOrEmpty(appPath))
            {
                paths.Add(appPath);
            }

            // 2. StartMenuInternet
            var clientKey = string.Format(StartMenuInternetRegistryKeyPattern, BrowserRegistryName);
            var clientPath = _settingsPort.GetSystemValue(clientKey, string.Empty)?.ToString();

            if (!string.IsNullOrEmpty(clientPath))
            {
                paths.Add(clientPath.Replace("\"", string.Empty).Trim());
            }

            // 3. Uninstall Keys
            AddPathFromUninstallKey(paths, string.Format(MachineUninstallRegistryKeyPattern, UninstallSubKey), ExeFileName, true);
            AddPathFromUninstallKey(paths, string.Format(Wow64UninstallRegistryKeyPattern, UninstallSubKey), ExeFileName, true);
            AddPathFromUninstallKey(paths, string.Format(UserUninstallRegistryKeyPattern, UninstallSubKey), ExeFileName, false);

            // 4. Standard default paths
            var pathPart = _fileSystemPort.CombinePaths(ProgramFilesSubPath, ExeFileName);

            paths.Add(_fileSystemPort.CombinePaths(_fileSystemPort.ResolveEnvironmentPath(ProgramFilesEnvVar), pathPart));
            paths.Add(_fileSystemPort.CombinePaths(_fileSystemPort.ResolveEnvironmentPath(ProgramFilesX86EnvVar), pathPart));
            paths.Add(_fileSystemPort.CombinePaths(_fileSystemPort.ResolveEnvironmentPath(LocalAppDataEnvVar), pathPart));

            return [.. paths.Distinct()];
        }
    }

    protected abstract string[] UserDataSubPath { get; }


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════

    public override bool IsExtensionInstalled(string? extensionId = null)
    {
        var idToCheck = string.IsNullOrWhiteSpace(extensionId) ? ExtensionId : extensionId;

        if (string.IsNullOrWhiteSpace(idToCheck))
        {
            return false;
        }

        var paths = ResolvePaths();

        if (paths.ExtensionsDirs is not { Count: > 0 })
        {
            return false;
        }

        foreach (var extensionsDir in paths.ExtensionsDirs)
        {
            if (!_fileSystemPort.DirectoryExists(extensionsDir))
            {
                continue;
            }

            if (extensionsDir.EndsWith(idToCheck, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

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
        var localAppData = _fileSystemPort.ResolveEnvironmentPath(LocalAppDataEnvVar);
        var userDataRoot = _fileSystemPort.CombinePaths([localAppData, .. UserDataSubPath]);

        List<string> cacheDirs = [];
        List<string> cookiesDirs = [];
        List<string> extensionsDirs = [];
        List<string> allProfileFolders = [];

        var defaultPath = _fileSystemPort.CombinePaths(userDataRoot, DefaultProfileFolderName);

        if (_fileSystemPort.DirectoryExists(defaultPath))
        {
            allProfileFolders.Add(defaultPath);
        }

        try
        {
            var dirs = _fileSystemPort.GetDirectories(userDataRoot, ProfileFolderSearchPattern);
            allProfileFolders.AddRange(dirs);
        }
        catch (Exception exception)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(exception, "Could not retrieve additional profile folders for Chromium user data path {UserDataRoot}", userDataRoot);
            }
        }

        foreach (var profilePath in allProfileFolders)
        {
            cacheDirs.Add(_fileSystemPort.CombinePaths(profilePath, CacheSubPath, CacheDataSubPath));
            cacheDirs.Add(_fileSystemPort.CombinePaths(profilePath, ServiceWorkerSubPath));
            cacheDirs.Add(_fileSystemPort.CombinePaths(profilePath, CodeCacheSubPath));
            cacheDirs.Add(_fileSystemPort.CombinePaths(profilePath, GpuCacheSubPath));

            cookiesDirs.Add(_fileSystemPort.CombinePaths(profilePath, IndexedDbSubPath));
            cookiesDirs.Add(_fileSystemPort.CombinePaths(profilePath, NetworkSubPath));
            cookiesDirs.Add(_fileSystemPort.CombinePaths(profilePath, LocalStorageSubPath));
            cookiesDirs.Add(_fileSystemPort.CombinePaths(profilePath, SessionStorageSubPath));

            extensionsDirs.Add(_fileSystemPort.CombinePaths(profilePath, ExtensionsFolderName, ExtensionId));
        }

        return new BrowserPaths(cacheDirs, cookiesDirs, extensionsDirs);
    }
}
