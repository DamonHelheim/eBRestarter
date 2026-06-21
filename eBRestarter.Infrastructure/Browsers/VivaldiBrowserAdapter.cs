using eBRestarter.Core.Application.Constants;
using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Infrastructure.Browsers.Abstract;
using Microsoft.Extensions.Logging;

namespace eBRestarter.Infrastructure.Browsers;

public class VivaldiBrowserAdapter(IOperatingSystemFacade os, ILogger<VivaldiBrowserAdapter> logger) : ChromiumBrowserBaseAdapter(os, logger)
{
    public override BrowserType Type => BrowserType.Vivaldi;
    public override string DisplayName => "Vivaldi";
    public override string IconPath => "ms-appx:///Resources/Visuals/Icons/Intersection/icons8_vivaldi.png"; // Bitte stelle sicher, dass dieses Icon existiert

    // Annahme: Du fÃ¼gst diesen Link noch in deine WebLinks Konstanten ein
    public override string DownloadUrl => WebLinks.VivaldiDownloadLinkDE;

    public override string ExtensionInstallUrl => WebLinks.ChromeEVisitorAddOnLink;

    public override string BrowserVersion
    {
        get
        {
            // 1. Pfade zum Uninstall-Key definieren
            string uninstallPathCU = $@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{UninstallSubKey}";
            string uninstallPathLM = $@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{UninstallSubKey}";
            string uninstallPathWow = $@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{UninstallSubKey}";

            // 2. Im Current User (HKCU) suchen (Standard bei Vivaldi)
            var version = _os.WindowsRegistryAdapter.RetrieveCurrentUserValue(uninstallPathCU, "DisplayVersion");

            // 3. Falls nicht gefunden, in Local Machine (HKLM) suchen
            version ??= _os.WindowsRegistryAdapter.RetrieveLocalMachineValue(uninstallPathLM, "DisplayVersion")
                ?? _os.WindowsRegistryAdapter.RetrieveLocalMachineValue(uninstallPathWow, "DisplayVersion");

            // 4. Wenn wir die Vivaldi-Version gefunden haben, bereinigen und zurÃ¼ckgeben
            if (version is not null && !string.IsNullOrEmpty(version.ToString()))
            {
                return CleanVersionString(version.ToString());
            }

            // 5. Fallback: Falls der Uninstall-Key (warum auch immer) fehlt,
            // greifen wir auf die Chromium-Logik aus der Basis-Klasse (BLBeacon) zurÃ¼ck.
            return base.BrowserVersion;
        }
    }

    // Implementierung der abstrakten Properties fÃ¼r die Suchstrategie
    protected override string ExeFileName => "vivaldi.exe";
    protected override string BrowserRegistryName => "Vivaldi";
    protected override string UninstallSubKey => "Vivaldi";
    protected override string ProgramFilesSubPath => @"Vivaldi\Application";

    public override string ProcessName => "vivaldi";

    // Vivaldi speichert Version oft im AutoUpdate Key oder Uninstall Key.
    // Falls "BLBeacon" bei Vivaldi nicht klappt, mÃ¼sstest du hier @"Software\Vivaldi" prÃ¼fen.
    protected override string RegistryKeyVersion => @"Software\Vivaldi\BLBeacon";
    // Vivaldi unterstÃ¼tzt Chrome-Erweiterungen direkt aus dem Chrome Web Store
    protected override string ExtensionId => "agchmcconfdfcenopioeilpgjngelefk";

    // Nutzt den Chrome Web Store Link


    public override BrowserPaths ResolvePaths()
    {
        var localAppData = _os.WindowsFileSystemServiceAdapter.ResolveEnvironmentPath("LocalAppData");

        // Das ist der Wurzel-Ordner fÃ¼r ALLE Daten bei Vivaldi
        // Pfad: C:\Users\Username\AppData\Local\Vivaldi\User Data
        var userDataRoot = _os.WindowsFileSystemServiceAdapter.CombinePaths(localAppData, "Vivaldi", "User Data");

        var cacheDirs = new List<string>();
        var cookiesDirs = new List<string>();
        var extensionsDirs = new List<string>();

        // 1. Wir suchen alle Profil-Ordner
        var allProfileFolders = new List<string>();

        // Check Default
        var defaultPath = _os.WindowsFileSystemServiceAdapter.CombinePaths(userDataRoot, "Default");
        if (_os.WindowsFileSystemServiceAdapter.DirectoryExists(defaultPath))
            allProfileFolders.Add(defaultPath);

        // Check Profile X
        try
        {
            var dirs = System.IO.Directory.GetDirectories(userDataRoot, "Profile *");
            allProfileFolders.AddRange(dirs);
        }
        catch { /* Fehlerbehandlung falls Ordner nicht existiert */ }

        // 2. FÃ¼r jedes gefundene Profil die Pfade generieren
        foreach (var profilePath in allProfileFolders)
        {
            // CACHE:
            cacheDirs.Add(_os.WindowsFileSystemServiceAdapter.CombinePaths(profilePath, "Cache", "Cache_Data"));
            cacheDirs.Add(_os.WindowsFileSystemServiceAdapter.CombinePaths(profilePath, "Service Worker"));
            cacheDirs.Add(_os.WindowsFileSystemServiceAdapter.CombinePaths(profilePath, "Code Cache"));
            cacheDirs.Add(_os.WindowsFileSystemServiceAdapter.CombinePaths(profilePath, "GPUCache"));

            // COOKIES:
            cookiesDirs.Add(_os.WindowsFileSystemServiceAdapter.CombinePaths(profilePath, "IndexedDB"));
            cookiesDirs.Add(_os.WindowsFileSystemServiceAdapter.CombinePaths(profilePath, "Network"));
            cookiesDirs.Add(_os.WindowsFileSystemServiceAdapter.CombinePaths(profilePath, "Local Storage"));
            cookiesDirs.Add(_os.WindowsFileSystemServiceAdapter.CombinePaths(profilePath, "Session Storage"));

            // EXTENSIONS:
            extensionsDirs.Add(_os.WindowsFileSystemServiceAdapter.CombinePaths(profilePath, "Extensions", ExtensionId));
        }

        return new BrowserPaths(cacheDirs, cookiesDirs, extensionsDirs);
    }
}






