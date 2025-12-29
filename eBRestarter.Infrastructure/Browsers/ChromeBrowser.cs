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
    public class ChromeBrowser : BrowserBase
    {
        public override BrowserType Type => BrowserType.Chrome;
        public override string DisplayName => "Google Chrome";
        public override string IconPath => "/Resources/Visuals/Icons/Intersection/fa_chrome.png";
        public override string DownloadUrl => WebLinks.ChromeDownloadLinkDE;
        protected override string ProcessName => "chrome";
        // Chrome speichert Version oft unter HKCU\BLBeacon
        protected override string RegistryKeyVersion => @"Software\Google\Chrome\BLBeacon";

        public ChromeBrowser(IOperatingSystemFacade os, ILogger<ChromeBrowser> logger) : base(os, logger) { }

        protected override List<string> ExecutablePaths =>
        [
            _os.WindowsFileSystemService.CombinePaths(_os.WindowsFileSystemService.GetEnvironmentPath("ProgramFiles"), @"Google\Chrome\Application\chrome.exe"),
            _os.WindowsFileSystemService.CombinePaths(_os.WindowsFileSystemService.GetEnvironmentPath("ProgramFiles(x86)"), @"Google\Chrome\Application\chrome.exe")
        ];

        public override BrowserPaths GetPaths()
        {
            var localAppData = _os.WindowsFileSystemService.GetEnvironmentPath("LocalAppData");
            var baseDir = _os.WindowsFileSystemService.CombinePaths(localAppData, @"Google\Chrome\User Data\Default");

            return new BrowserPaths(
                CacheDir: _os.WindowsFileSystemService.CombinePaths(baseDir, "Cache"),
                CookiesDir: _os.WindowsFileSystemService.CombinePaths(baseDir, "Network"),
                ExtensionsDir: _os.WindowsFileSystemService.CombinePaths(baseDir, "Extensions")
            );
        }
    }
}
