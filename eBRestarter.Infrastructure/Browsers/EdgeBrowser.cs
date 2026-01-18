using eBRestarter.Core.Application.Contstants;
using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Domain.Enums;
using eBRestarter.Core.Domain.Models.Records;
using eBRestarter.Infrastructure.Browsers.Abstract;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Infrastructure.Browsers
{
    public class EdgeBrowser : ChromiumBrowserBase
    {
        public override string DisplayName => "Edge";
        public override string IconPath => "ms-appx:///Resources/Visuals/Icons/Intersection/fa_edge.png";
        public override string DownloadUrl => WebLinks.EdgeDownloadLinkDE;

        public override BrowserType Type => BrowserType.Edge;

        // Edge nutzt "msedge" als Prozessnamen
        protected override string ProcessName => "msedge";

        // Registry Key für Versionsprüfung (ähnlich Chrome)
        protected override string RegistryKeyVersion => @"Software\Microsoft\Edge\BLBeacon";

        // --- NEU IMPLEMENTIERT ---
        protected override string ExtensionId => "agchmcconfdfcenopioeilpgjngelefk";
        public override string ExtensionInstallUrl => WebLinks.EdgeEVisitorAddOnLink;//"https://chrome.google.com/webstore/detail/ebesucher-addon/agchmcconfdfcenopioeilpgjngelefk";

        public EdgeBrowser(IOperatingSystemFacade os, ILogger<EdgeBrowser> logger) : base(os, logger) { }

        // WICHTIG: Die Exe heißt msedge.exe, nicht edge.exe!
        protected override string ExeFileName => "msedge.exe";

        // HKLM\SOFTWARE\Clients\StartMenuInternet\Microsoft Edge
        protected override string BrowserRegistryName => "Microsoft Edge";

        // HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Microsoft Edge
        protected override string UninstallSubKey => "Microsoft Edge";

        // C:\Program Files (x86)\Microsoft\Edge\Application
        protected override string ProgramFilesSubPath => @"Microsoft\Edge\Application";

        public override BrowserPaths GetPaths()
        {
            var localAppData = _os.WindowsFileSystemService.GetEnvironmentPath("LocalAppData");

            // Das ist der Wurzel-Ordner für ALLE Daten
            var userDataRoot = _os.WindowsFileSystemService.CombinePaths(localAppData, "Microsoft", "Edge", "User Data");

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

                // COOKIES: Du wolltest "IndexedDB" (und meistens "Network")
                // IndexedDB:
                cookiesDirs.Add(_os.WindowsFileSystemService.CombinePaths(profilePath, "IndexedDB"));
                // Network (Hier liegen die echten Cookies in der Datei 'Cookies'):
                cookiesDirs.Add(_os.WindowsFileSystemService.CombinePaths(profilePath, "Network"));

                // EXTENSIONS:
                extensionsDirs.Add(_os.WindowsFileSystemService.CombinePaths(profilePath, "Extensions", ExtensionId));
            }

            return new BrowserPaths(cacheDirs, cookiesDirs, extensionsDirs);
        }

        //public override BrowserPaths GetPaths()
        //{
        //    var localAppData = _os.WindowsFileSystemService.GetEnvironmentPath("LocalAppData");
        //    // Edge User Data liegt typischerweise in LocalAppData\Microsoft\Edge\User Data\Default
        //    var baseDir = _os.WindowsFileSystemService.CombinePaths(localAppData, "Microsoft", "Edge", "User Data", "Default");

        //    return new BrowserPaths(
        //        CacheDir: _os.WindowsFileSystemService.CombinePaths(baseDir, "Cache"),
        //        CookiesDir: _os.WindowsFileSystemService.CombinePaths(baseDir, "IndexedDB"), // Edge nutzt oft IndexedDB für die Struktur ähnlich Chrome
        //        ExtensionsDir: _os.WindowsFileSystemService.CombinePaths(baseDir, "Extensions", ExtensionId)
        //    );
        //}

        //C:\Users\Workstation\AppData\Local\Microsoft\Edge\User Data\Profile 1\IndexedDB
        //C:\Users\Workstation\AppData\Local\Microsoft\Edge\User Data\Profile 1\Service Worker
        //C:\Users\Workstation\AppData\Local\Microsoft\Edge\User Data\Profile 4\Cache
        //C:\Users\Workstation\AppData\Local\Microsoft\Edge\User Data\Profile 4\Network
    }
}
