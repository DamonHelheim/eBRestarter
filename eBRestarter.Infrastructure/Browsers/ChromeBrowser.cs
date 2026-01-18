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
    public class ChromeBrowser : ChromiumBrowserBase
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
        public ChromeBrowser(IOperatingSystemFacade os, ILogger<ChromeBrowser> logger) : base(os, logger) { }


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
        //    var baseDir = _os.WindowsFileSystemService.CombinePaths(localAppData, @"Google\Chrome\User Data\Default");

        //    return new BrowserPaths(
        //        CacheDir: _os.WindowsFileSystemService.CombinePaths(baseDir, "Cache"),
        //        CookiesDir: _os.WindowsFileSystemService.CombinePaths(baseDir, "Network"),
        //        ExtensionsDir: _os.WindowsFileSystemService.CombinePaths(baseDir, "Extensions")
        //    );
        //}
    }
}
