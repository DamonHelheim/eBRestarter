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
        public override string IconPath => "/Resources/Visuals/Icons/Intersection/fa_edge.png";
        public override string DownloadUrl => WebLinks.EdgeDownloadLinkDE;

        public override BrowserType Type => BrowserType.Edge;

        // Edge nutzt "msedge" als Prozessnamen
        protected override string ProcessName => "msedge";

        // Registry Key für Versionsprüfung (ähnlich Chrome)
        protected override string RegistryKeyVersion => @"Software\Microsoft\Edge\BLBeacon";

        // --- NEU IMPLEMENTIERT ---
        protected override string ExtensionId => "agchmcconfdfcenopioeilpgjngelefk";
        public override string ExtensionInstallUrl => "https://chrome.google.com/webstore/detail/ebesucher-addon/agchmcconfdfcenopioeilpgjngelefk";

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
            // Edge User Data liegt typischerweise in LocalAppData\Microsoft\Edge\User Data\Default
            var baseDir = _os.WindowsFileSystemService.CombinePaths(localAppData, "Microsoft", "Edge", "User Data", "Default");

            return new BrowserPaths(
                CacheDir: _os.WindowsFileSystemService.CombinePaths(baseDir, "Cache"),
                CookiesDir: _os.WindowsFileSystemService.CombinePaths(baseDir, "IndexedDB"), // Edge nutzt oft IndexedDB für die Struktur ähnlich Chrome
                ExtensionsDir: _os.WindowsFileSystemService.CombinePaths(baseDir, "Extensions", ExtensionId)
            );
        }
    }
}
