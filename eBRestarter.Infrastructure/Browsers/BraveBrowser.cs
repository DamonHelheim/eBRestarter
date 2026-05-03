using eBRestarter.Core.Application.Constants;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Infrastructure.Browsers.Abstract;
using Microsoft.Extensions.Logging;

namespace eBRestarter.Infrastructure.Browsers;

public class BraveBrowser(IOperatingSystemFacade os, ILogger<BraveBrowser> logger) : ChromiumBrowserBase(os, logger)
{

    protected override string ExeFileName => "brave.exe";
    protected override string BrowserRegistryName => "Brave"; // Oder "BraveSoftware Brave-Browser", je nach Registry
    protected override string UninstallSubKey => "BraveSoftware Brave-Browser";
    protected override string ProgramFilesSubPath => @"BraveSoftware\Brave-Browser\Application";

    public override string DisplayName => "Brave";
    public override string IconPath => "ms-appx:///Resources/Visuals/Icons/Intersection/fa_brave.png";
    public override string DownloadUrl => WebLinks.BraveDownloadLinkDE;

    public override BrowserType Type => BrowserType.Brave;

    // Brave nutzt "brave" als Prozessnamen
    protected override string ProcessName => "brave";

    // Registry Key für Versionsprüfung
    protected override string RegistryKeyVersion => @"Software\BraveSoftware\Brave-Browser\BLBeacon";

    // --- NEU IMPLEMENTIERT ---
    protected override string ExtensionId => "agchmcconfdfcenopioeilpgjngelefk";
    public override string ExtensionInstallUrl => "https://chrome.google.com/webstore/detail/ebesucher-addon/agchmcconfdfcenopioeilpgjngelefk";

    public override BrowserPaths GetPaths()
    {
        var localAppData = _os.WindowsFileSystemService.GetEnvironmentPath("LocalAppData");

        // Das ist der Wurzel-Ordner für ALLE Daten
        var userDataRoot = _os.WindowsFileSystemService.CombinePaths(localAppData, "BraveSoftware", "Brave-Browser", "User Data");

        var cacheDirs = new List<string>();
        var cookiesDirs = new List<string>();
        var extensionsDirs = new List<string>();

        // 1. Wir suchen alle Profil-Ordner
        // Wir nehmen 'Default' UND alle Ordner, die mit 'Profile' beginnen (z.B. 'Profile 1')
        var allProfileFolders = new List<string>();

        // Check Default
        var defaultPath = _os.WindowsFileSystemService.CombinePaths(userDataRoot, "Default");

        if (_os.WindowsFileSystemService.DirectoryExists(defaultPath))
        {
            allProfileFolders.Add(defaultPath);

        }

        // Check Profile X (Dafür bräuchtest du eigentlich Directory.GetDirectories,
        // ich nutze hier eine fiktive Methode deines FileServices oder System.IO)
        // Da deine IWindowsFileSystemService-Schnittstelle hier nicht voll sichtbar ist,
        // nutzen wir System.IO direkt oder du musst es in deinen Service wrappen:
        try
        {
            var dirs = Directory.GetDirectories(userDataRoot, "Profile *");

            allProfileFolders.AddRange(dirs);
        }
        catch { /* Fehlerbehandlung falls Ordner nicht existiert */ }

        // 2. Für jedes gefundene Profil die Pfade generieren
        foreach (var profilePath in allProfileFolders)
        {
            // CACHE: Du wolltest speziell "Service Worker" (und meistens auch "Cache")
            // Standard-Cache:
            cacheDirs.Add(_os.WindowsFileSystemService.CombinePaths(profilePath, "Cache", "Cache_Data"));
            // Service Worker (wie von dir angefordert):
            cacheDirs.Add(_os.WindowsFileSystemService.CombinePaths(profilePath, "Service Worker"));

            // Code Cache (JS/Wasm)
            cacheDirs.Add(_os.WindowsFileSystemService.CombinePaths(profilePath, "Code Cache"));
            // GPU Cache (Grafik)
            cacheDirs.Add(_os.WindowsFileSystemService.CombinePaths(profilePath, "GPUCache"));

            // COOKIES: Du wolltest "IndexedDB" (und meistens "Network")
            // IndexedDB:
            cookiesDirs.Add(_os.WindowsFileSystemService.CombinePaths(profilePath, "IndexedDB"));
            // Network (Hier liegen die echten Cookies in der Datei 'Cookies'):
            cookiesDirs.Add(_os.WindowsFileSystemService.CombinePaths(profilePath, "Network"));

            // Local Storage (WICHTIG!)
            cookiesDirs.Add(_os.WindowsFileSystemService.CombinePaths(profilePath, "Local Storage"));
            // Session Storage
            cookiesDirs.Add(_os.WindowsFileSystemService.CombinePaths(profilePath, "Session Storage"));

            // EXTENSIONS:
            extensionsDirs.Add(_os.WindowsFileSystemService.CombinePaths(profilePath, "Extensions", ExtensionId));
        }

        return new BrowserPaths(cacheDirs, cookiesDirs, extensionsDirs);
    }

    //C:\Users\Workstation\AppData\Local\BraveSoftware\Brave-Browser\User Data\Default\IndexedDB
    //C:\Users\Workstation\AppData\Local\BraveSoftware\Brave-Browser\User Data\Default\Service Worker
    //C:\Users\Workstation\AppData\Local\BraveSoftware\Brave-Browser\User Data\Default\Cache
    //C:\Users\Workstation\AppData\Local\BraveSoftware\Brave-Browser\User Data\Default\Network
}
