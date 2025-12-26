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
    public class BraveBrowser : BrowserBase
    {
        public override BrowserType Type => BrowserType.Brave;

        // Brave nutzt "brave" als Prozessnamen
        protected override string ProcessName => "brave";

        // Registry Key für Versionsprüfung
        protected override string RegistryKeyVersion => @"Software\BraveSoftware\Brave-Browser\BLBeacon";

        // Konstanten spezifisch für Brave
        private const string ExtensionId = "agchmcconfdfcenopioeilpgjngelefk";

        public BraveBrowser(IOperatingSystemFacade os, ILogger<BraveBrowser> logger)
            : base(os, logger) { }

        protected override List<string> ExecutablePaths => new()
        {
            _os.WindowsFileSystemService.CombinePaths(_os.WindowsFileSystemService.GetEnvironmentPath("ProgramFiles"), @"BraveSoftware\Brave-Browser\Application\brave.exe"),
            _os.WindowsFileSystemService.CombinePaths(_os.WindowsFileSystemService.GetEnvironmentPath("ProgramFiles(x86)"), @"BraveSoftware\Brave-Browser\Application\brave.exe")
        };

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
