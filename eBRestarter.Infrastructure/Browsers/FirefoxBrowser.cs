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
        public override string DownloadUrl => WebLinks.FirefoxDownloadLink; // Stellen Sie sicher, dass WebLinks existiert
        public override BrowserType Type => BrowserType.Firefox;
        protected override string ProcessName => "firefox";

        // Basis-Pfad ohne Version für spätere Versions-Checks
        protected override string RegistryKeyVersion => @"Software\Mozilla\Mozilla Firefox";

        public FirefoxBrowser(IOperatingSystemFacade os, ILogger<FirefoxBrowser> logger)
            : base(os, logger) { }

        protected override List<string> ExecutablePaths
        {
            get
            {
                var paths = new List<string>();

                // ---------------------------------------------------------
                // 1. REGISTRY: Dynamic Lookup (CurrentVersion -> Main -> PathToExe)
                // ---------------------------------------------------------

                // Lokale Hilfsfunktion, um den zweistufigen Lookup zu machen
                string? GetPathFromMozillaRegistry(bool isHklm)
                {
                    // Schritt A: Den Wert "CurrentVersion" auslesen
                    // Pfad: Software\Mozilla\Mozilla Firefox
                    string rootKey = @"Software\Mozilla\Mozilla Firefox";

                    var currentVersionObj = isHklm
                        ? _os.WindowsRegistryService.GetLocalMachineValue(rootKey, "CurrentVersion")
                        : _os.WindowsRegistryService.GetCurrentUserValue(rootKey, "CurrentVersion");

                    if (currentVersionObj == null) return null;

                    string currentVersion = currentVersionObj.ToString()!;
                    // Ergebnis z.B.: "146.0.1 (x64 de)"

                    // Schritt B: Den Pfad zum "Main" Schlüssel bauen
                    // Pfad: Software\Mozilla\Mozilla Firefox\146.0.1 (x64 de)\Main
                    string mainKeyPath = $@"{rootKey}\{currentVersion}\Main";

                    // Schritt C: Den Wert "PathToExe" auslesen
                    var pathToExeObj = isHklm
                        ? _os.WindowsRegistryService.GetLocalMachineValue(mainKeyPath, "PathToExe")
                        : _os.WindowsRegistryService.GetCurrentUserValue(mainKeyPath, "PathToExe");

                    return pathToExeObj?.ToString();
                }

                // Suche in HKCU
                var hkcuPath = GetPathFromMozillaRegistry(false);
                if (!string.IsNullOrEmpty(hkcuPath)) paths.Add(hkcuPath);

                // Suche in HKLM (Falls als Admin für alle User installiert)
                var hklmPath = GetPathFromMozillaRegistry(true);
                if (!string.IsNullOrEmpty(hklmPath)) paths.Add(hklmPath);


                // ---------------------------------------------------------
                // 2. STANDARDPFADE (Fallback)
                // ---------------------------------------------------------
                paths.Add(_os.WindowsFileSystemService.CombinePaths(_os.WindowsFileSystemService.GetEnvironmentPath("ProgramFiles"), @"Mozilla Firefox\firefox.exe"));
                paths.Add(_os.WindowsFileSystemService.CombinePaths(_os.WindowsFileSystemService.GetEnvironmentPath("ProgramFiles(x86)"), @"Mozilla Firefox\firefox.exe"));

                // Firefox kann auch im AppData liegen (User-Install ohne Admin)
                paths.Add(_os.WindowsFileSystemService.CombinePaths(_os.WindowsFileSystemService.GetEnvironmentPath("LocalAppData"), @"Mozilla Firefox\firefox.exe"));

                return [.. paths.Distinct()];
            }
        }

        public override BrowserPaths GetPaths()
        {
            var appData = _os.WindowsFileSystemService.GetEnvironmentPath("AppData"); // Roaming!
            var firefoxRoot = _os.WindowsFileSystemService.CombinePaths(appData, "Mozilla", "Firefox");

            // Profilname ermitteln
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
            // Einfacher Fallback, falls Datei nicht existiert oder Parsing zu komplex für diesen Ausschnitt ist
            if (!_os.WindowsFileSystemService.FileExists(iniPath)) return "Default";

            // Hinweis: Hier müsste eine Logik rein, die die profiles.ini parst, 
            // nach "Path=Profiles/xxxx.default" sucht und das "Profiles/" abschneidet.
            // Für jetzt geben wir einen Platzhalter zurück oder man müsste IFileSystemService um ReadAllLines erweitern.

            return "Default";
        }
    }
}
