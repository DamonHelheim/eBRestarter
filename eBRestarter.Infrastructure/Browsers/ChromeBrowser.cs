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
        public override string IconPath => "/Resources/Visuals/Icons/Intersection/fa_chrome.png";
        public override string DownloadUrl => WebLinks.ChromeDownloadLinkDE;
        protected override string ProcessName => "chrome";
        // Chrome speichert Version oft unter HKCU\BLBeacon
        protected override string RegistryKeyVersion => @"Software\Google\Chrome\BLBeacon";
        // --- NEU IMPLEMENTIERT ---
        protected override string ExtensionId => "agchmcconfdfcenopioeilpgjngelefk";
        public override string ExtensionInstallUrl => "https://chrome.google.com/webstore/detail/ebesucher-addon/agchmcconfdfcenopioeilpgjngelefk";
        public ChromeBrowser(IOperatingSystemFacade os, ILogger<ChromeBrowser> logger) : base(os, logger) { }

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
