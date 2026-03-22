using eBRestarter.Core.Application.Contstants;
using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Domain.Enums;
using eBRestarter.Core.Domain.Models.Records;
using eBRestarter.Infrastructure.Browsers.Abstract;
using Microsoft.Extensions.Logging;

namespace eBRestarter.Infrastructure.Browsers;

public class ChromeBrowser(IOperatingSystemFacade os, ILogger<ChromeBrowser> logger) : ChromiumBrowserBase(os, logger)
{

    // Implementierung der abstrakten Properties für die Suchstrategie
    protected override string ExeFileName => "chrome.exe";
    protected override string BrowserRegistryName => "Google Chrome";
    protected override string UninstallSubKey => "Google Chrome";
    protected override string ProgramFilesSubPath => @"Google\Chrome\Application";

    public override BrowserType Type => BrowserType.Chrome;
    public override string DisplayName => "Chrome";
    public override string IconPath => "ms-appx:///Resources/Visuals/Icons/Intersection/fa_chrome.png";
    public override string DownloadUrl => WebLinks.ChromeDownloadLinkDE;
    protected override string ProcessName => "chrome";
    // Chrome speichert Version oft unter HKCU\BLBeacon
    protected override string RegistryKeyVersion => @"Software\Google\Chrome\BLBeacon";
    // --- NEU IMPLEMENTIERT ---
    protected override string ExtensionId => "agchmcconfdfcenopioeilpgjngelefk";
    public override string ExtensionInstallUrl => WebLinks.ChromeEVisitorAddOnLink;//"https://chrome.google.com/webstore/detail/ebesucher-addon/agchmcconfdfcenopioeilpgjngelefk";


    public override BrowserPaths GetPaths()
    {
        var localAppData = _os.WindowsFileSystemService.GetEnvironmentPath("LocalAppData");

        // Das ist der Wurzel-Ordner für ALLE Daten
        var userDataRoot = _os.WindowsFileSystemService.CombinePaths(localAppData, "Google", "Chrome", "User Data");

        var cacheDirs = new List<string>();
        var cookiesDirs = new List<string>();
        var extensionsDirs = new List<string>();

        // 1. Wir suchen alle Profil-Ordner
        // Wir nehmen 'Default' UND alle Ordner, die mit 'Profile' beginnen (z.B. 'Profile 1')
        var allProfileFolders = new List<string>();

        // Check Default
        var defaultPath = _os.WindowsFileSystemService.CombinePaths(userDataRoot, "Default");
        if (_os.WindowsFileSystemService.DirectoryExists(defaultPath))
            allProfileFolders.Add(defaultPath);

        // Check Profile X (Dafür bräuchtest du eigentlich Directory.GetDirectories,
        // ich nutze hier eine fiktive Methode deines FileServices oder System.IO)
        // Da deine IWindowsFileSystemService-Schnittstelle hier nicht voll sichtbar ist,
        // nutzen wir System.IO direkt oder du musst es in deinen Service wrappen:
        try
        {
            var dirs = System.IO.Directory.GetDirectories(userDataRoot, "Profile *");
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

    //C:\Users\Workstation\AppData\Local\Google\Chrome\User Data\Default\IndexedDB
    //C:\Users\Workstation\AppData\Local\Google\Chrome\User Data\Default\Service Worker

    //C:\Users\Workstation\AppData\Local\Google\Chrome\User Data\Default\Cache
    //C:\Users\Workstation\AppData\Local\Google\Chrome\User Data\Default\Code Cache
    //C:\Users\Workstation\AppData\Local\Google\Chrome\User Data\Default\Network

    //Eine SQLite-Datenbank, die alle besuchten URLs enthält.
    //"C:\Users\Workstation\AppData\Local\Google\Chrome\User Data\Default\History"

    //Hier wird gespeichert, welche Tabs offen sind. Wenn du das löschst, startet der Browser "leer" (keine "Zuletzt geschlossene Tabs wiederherstellen").
    //C:\Users\Workstation\AppData\Local\Google\Chrome\User Data\Default\Sessions
}
