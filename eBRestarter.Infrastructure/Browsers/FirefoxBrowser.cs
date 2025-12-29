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
    public class FirefoxBrowser : BrowserBase
    {
        public override string DisplayName => "Firefox";
        public override string IconPath => "/Resources/Visuals/Icons/Intersection/fa_firefox.png";
        public override string DownloadUrl => WebLinks.FirefoxDownloadLink;
        public override BrowserType Type => BrowserType.Firefox;
        protected override string ProcessName => "firefox";
        protected override string RegistryKeyVersion => @"SOFTWARE\Mozilla\Mozilla Firefox";

        public FirefoxBrowser(IOperatingSystemFacade os, ILogger<FirefoxBrowser> logger)
            : base(os, logger) { }

        protected override List<string> ExecutablePaths => new()
        {
             _os.WindowsFileSystemService.CombinePaths(_os.WindowsFileSystemService.GetEnvironmentPath("ProgramFiles"), @"Mozilla Firefox\firefox.exe"),
             _os.WindowsFileSystemService.CombinePaths(_os.WindowsFileSystemService.GetEnvironmentPath("ProgramFiles(x86)"), @"Mozilla Firefox\firefox.exe")
        };

        public override BrowserPaths GetPaths()
        {
            var appData = _os.WindowsFileSystemService.GetEnvironmentPath("AppData"); // Roaming!
            var firefoxRoot = _os.WindowsFileSystemService.CombinePaths(appData, "Mozilla", "Firefox");

            // Profilname ermitteln (Logik unten)
            var profileName = GetProfileNameFromIni(_os.WindowsFileSystemService.CombinePaths(firefoxRoot, "profiles.ini"));

            var profilePathRoaming = _os.WindowsFileSystemService.CombinePaths(firefoxRoot, "Profiles", profileName);
            var profilePathLocal = _os.WindowsFileSystemService.CombinePaths(_os.WindowsFileSystemService.GetEnvironmentPath("LocalAppData"), "Mozilla", "Firefox", "Profiles", profileName);

            return new BrowserPaths(
                CacheDir: _os.WindowsFileSystemService.CombinePaths(profilePathLocal, "cache2", "entries"),
                CookiesDir: _os.WindowsFileSystemService.CombinePaths(profilePathRoaming, "storage", "default"),
                ExtensionsDir: _os.WindowsFileSystemService.CombinePaths(profilePathRoaming, "extensions")
            );
        }

        private string GetProfileNameFromIni(string iniPath)
        {
            if (!_os.WindowsFileSystemService.FileExists(iniPath)) return "Default";

            // Annahme: IWindowsFileSystemServiceService hat ReadAllLines. Wenn nicht, ergänzen!
            // var lines = _os.WindowsFileSystemService.ReadAllLines(iniPath);
            // ... Parsing Logik für "Path=Profiles/xxxx.default" ...

            return "Default.default-release"; // Fallback für dieses Beispiel
        }
    }
}
