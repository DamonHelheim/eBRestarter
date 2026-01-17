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
    public class BraveBrowser : ChromiumBrowserBase
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

        public BraveBrowser(IOperatingSystemFacade os, ILogger<BraveBrowser> logger) : base(os, logger) { }

        public override BrowserPaths GetPaths()
        {
            var localAppData = _os.WindowsFileSystemService.GetEnvironmentPath("LocalAppData");
            // Brave User Data liegt in LocalAppData\BraveSoftware\Brave-Browser\User Data\Default
            var baseDir = _os.WindowsFileSystemService.CombinePaths(localAppData, "BraveSoftware", "Brave-Browser", "User Data", "Default");

            return new BrowserPaths(
                CacheDir: _os.WindowsFileSystemService.CombinePaths(baseDir, "Cache"),
                CookiesDir: _os.WindowsFileSystemService.CombinePaths(baseDir, "Network"), // Chromium Standard ist meist Network oder IndexedDB
                ExtensionsDir: _os.WindowsFileSystemService.CombinePaths(baseDir, "Extensions", ExtensionId)
            );
        }
    }
}
