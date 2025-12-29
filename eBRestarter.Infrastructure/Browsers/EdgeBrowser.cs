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
    public class EdgeBrowser : BrowserBase
    {
        public override string DisplayName => "Edge";
        public override string IconPath => "/Resources/Images/Icons/Intersection/fa_chrome.png";
        public override string DownloadUrl => WebLinks.EdgeDownloadLinkDE;

        public override BrowserType Type => BrowserType.Edge;

        // Edge nutzt "msedge" als Prozessnamen
        protected override string ProcessName => "msedge";

        // Registry Key für Versionsprüfung (ähnlich Chrome)
        protected override string RegistryKeyVersion => @"Software\Microsoft\Edge\BLBeacon";

        // Konstanten spezifisch für Edge
        private const string ExtensionId = "kjhejmaladginnedpoppohfnkionnghi";

        public EdgeBrowser(IOperatingSystemFacade os, ILogger<EdgeBrowser> logger) : base(os, logger) { }

        protected override List<string> ExecutablePaths => new()
        {
            // Standard Pfade für x64 und x86
            _os.WindowsFileSystemService.CombinePaths(_os.WindowsFileSystemService.GetEnvironmentPath("ProgramFiles(x86)"), @"Microsoft\Edge\Application\msedge.exe"),
            _os.WindowsFileSystemService.CombinePaths(_os.WindowsFileSystemService.GetEnvironmentPath("ProgramFiles"), @"Microsoft\Edge\Application\msedge.exe")
        };

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
