using eBRestarter.Core.Application.Contstants;
using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Domain.Enums;
using eBRestarter.Core.Domain.Models.Records;
using eBRestarter.Infrastructure.Browsers.Abstract;
using Microsoft.Extensions.Logging;

namespace eBRestarter.Infrastructure.Browsers;

public class VivaldiBrowser(IOperatingSystemFacade os, ILogger<VivaldiBrowser> logger) : ChromiumBrowserBase(os, logger)
{
    // Implementierung der abstrakten Properties für die Suchstrategie
    protected override string ExeFileName => "vivaldi.exe";
    protected override string BrowserRegistryName => "Vivaldi";
    protected override string UninstallSubKey => "Vivaldi";
    protected override string ProgramFilesSubPath => @"Vivaldi\Application";

    public override BrowserType Type => BrowserType.Vivaldi;
    public override string DisplayName => "Vivaldi";
    public override string IconPath => "ms-appx:///Resources/Visuals/Icons/Intersection/icons8_vivaldi.png"; // Bitte stelle sicher, dass dieses Icon existiert

    // Annahme: Du fügst diesen Link noch in deine WebLinks Konstanten ein
    public override string DownloadUrl => WebLinks.VivaldiDownloadLinkDE;

    protected override string ProcessName => "vivaldi";

    // Vivaldi speichert Version oft im AutoUpdate Key oder Uninstall Key.
    // Falls "BLBeacon" bei Vivaldi nicht klappt, müsstest du hier @"Software\Vivaldi" prüfen.
    protected override string RegistryKeyVersion => @"Software\Vivaldi\BLBeacon";

    // --- Extension / Add-On ---
    // Vivaldi unterstützt Chrome-Erweiterungen direkt aus dem Chrome Web Store
    protected override string ExtensionId => "agchmcconfdfcenopioeilpgjngelefk";

    // Nutzt den Chrome Web Store Link
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
            var version = _os.WindowsRegistryService.GetCurrentUserValue(uninstallPathCU, "DisplayVersion");

            // 3. Falls nicht gefunden, in Local Machine (HKLM) suchen
            version ??= _os.WindowsRegistryService.GetLocalMachineValue(uninstallPathLM, "DisplayVersion")
                       ?? _os.WindowsRegistryService.GetLocalMachineValue(uninstallPathWow, "DisplayVersion");

            // 4. Wenn wir die Vivaldi-Version gefunden haben, bereinigen und zurückgeben
            if (version != null && !string.IsNullOrEmpty(version.ToString()))
            {
                return CleanVersionString(version.ToString());
            }

            // 5. Fallback: Falls der Uninstall-Key (warum auch immer) fehlt,
            // greifen wir auf die Chromium-Logik aus der Basis-Klasse (BLBeacon) zurück.
            return base.BrowserVersion;
        }
    }

    public override BrowserPaths GetPaths()
    {
        var localAppData = _os.WindowsFileSystemService.GetEnvironmentPath("LocalAppData");

        // Das ist der Wurzel-Ordner für ALLE Daten bei Vivaldi
        // Pfad: C:\Users\Username\AppData\Local\Vivaldi\User Data
        var userDataRoot = _os.WindowsFileSystemService.CombinePaths(localAppData, "Vivaldi", "User Data");

        var cacheDirs = new List<string>();
        var cookiesDirs = new List<string>();
        var extensionsDirs = new List<string>();

        // 1. Wir suchen alle Profil-Ordner
        var allProfileFolders = new List<string>();

        // Check Default
        var defaultPath = _os.WindowsFileSystemService.CombinePaths(userDataRoot, "Default");
        if (_os.WindowsFileSystemService.DirectoryExists(defaultPath))
            allProfileFolders.Add(defaultPath);

        // Check Profile X
        try
        {
            var dirs = System.IO.Directory.GetDirectories(userDataRoot, "Profile *");
            allProfileFolders.AddRange(dirs);
        }
        catch { /* Fehlerbehandlung falls Ordner nicht existiert */ }

        // 2. Für jedes gefundene Profil die Pfade generieren
        foreach (var profilePath in allProfileFolders)
        {
            // CACHE:
            cacheDirs.Add(_os.WindowsFileSystemService.CombinePaths(profilePath, "Cache", "Cache_Data"));
            cacheDirs.Add(_os.WindowsFileSystemService.CombinePaths(profilePath, "Service Worker"));
            cacheDirs.Add(_os.WindowsFileSystemService.CombinePaths(profilePath, "Code Cache"));
            cacheDirs.Add(_os.WindowsFileSystemService.CombinePaths(profilePath, "GPUCache"));

            // COOKIES:
            cookiesDirs.Add(_os.WindowsFileSystemService.CombinePaths(profilePath, "IndexedDB"));
            cookiesDirs.Add(_os.WindowsFileSystemService.CombinePaths(profilePath, "Network"));
            cookiesDirs.Add(_os.WindowsFileSystemService.CombinePaths(profilePath, "Local Storage"));
            cookiesDirs.Add(_os.WindowsFileSystemService.CombinePaths(profilePath, "Session Storage"));

            // EXTENSIONS:
            extensionsDirs.Add(_os.WindowsFileSystemService.CombinePaths(profilePath, "Extensions", ExtensionId));
        }

        return new BrowserPaths(cacheDirs, cookiesDirs, extensionsDirs);
    }
}