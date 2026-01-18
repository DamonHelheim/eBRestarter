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
        // --- Konstanten ---
        private const string EbesucherAddOnNameForFirefox = "{fef425dc-a60f-4484-954d-71ecf2544846}.xpi";

        // --- Properties ---
        public override string DisplayName => "Firefox";
        public override string IconPath => "ms-appx:///Resources/Visuals/Icons/Intersection/fa_firefox.png";
        public override string DownloadUrl => WebLinks.FirefoxDownloadLink;
        public override string ExtensionInstallUrl => WebLinks.FirefoxEVisitorAddOnLink;
        public override BrowserType Type => BrowserType.Firefox;
        protected override string ProcessName => "firefox";
        protected override string RegistryKeyVersion => @"Software\Mozilla\Mozilla Firefox";

        public FirefoxBrowser(IOperatingSystemFacade os, ILogger<FirefoxBrowser> logger)
            : base(os, logger) { }

        // ---------------------------------------------------------------------------------
        // PRÜFUNG: Ist die Extension installiert? (Sucht in ALLEN Profilen)
        // ---------------------------------------------------------------------------------
        public override bool IsExtensionInstalled(string? extensionId = null)
        {
            var paths = GetPaths();

            // Wenn keine Extensions-Ordner gefunden wurden, abbrechen
            if (paths.ExtensionsDirs == null || paths.ExtensionsDirs.Count == 0) return false;

            var idToCheck = string.IsNullOrEmpty(extensionId)
                ? EbesucherAddOnNameForFirefox.TrimStart('\\')
                : extensionId;

            // Wir prüfen jeden gefundenen Profil-Ordner
            foreach (var extensionsDir in paths.ExtensionsDirs)
            {
                if (!_os.WindowsFileSystemService.DirectoryExists(extensionsDir)) continue;

                // Fall 1: Die Extension ist eine .xpi Datei
                var xpiPath = _os.WindowsFileSystemService.CombinePaths(extensionsDir, idToCheck);
                if (_os.WindowsFileSystemService.FileExists(xpiPath)) return true;

                // Fall 2: Die Extension ist ein entpackter Ordner (Sideloading)
                // Entferne .xpi Endung für den Ordnernamen-Check
                var folderName = idToCheck.Replace(".xpi", "", StringComparison.OrdinalIgnoreCase);
                var folderPath = _os.WindowsFileSystemService.CombinePaths(extensionsDir, folderName);
                if (_os.WindowsFileSystemService.DirectoryExists(folderPath)) return true;
            }

            return false;
        }

        // ---------------------------------------------------------------------------------
        // PFADE: Cache, Cookies & Extensions für ALLE Profile ermitteln
        // ---------------------------------------------------------------------------------
        public override BrowserPaths GetPaths()
        {
            var appData = _os.WindowsFileSystemService.GetEnvironmentPath("AppData"); // Roaming
            var localAppData = _os.WindowsFileSystemService.GetEnvironmentPath("LocalAppData"); // Local

            var firefoxRoamingRoot = _os.WindowsFileSystemService.CombinePaths(appData, "Mozilla", "Firefox");
            var firefoxLocalRoot = _os.WindowsFileSystemService.CombinePaths(localAppData, "Mozilla", "Firefox");

            // Listen für die Ergebnisse
            var cacheDirs = new List<string>();
            var cookiesDirs = new List<string>();
            var extensionsDirs = new List<string>();

            // 1. profiles.ini einlesen und alle Profil-Ordner finden
            var profilesIniPath = _os.WindowsFileSystemService.CombinePaths(firefoxRoamingRoot, "profiles.ini");
            var profileInfos = GetProfileFoldersFromIni(profilesIniPath);

            // 2. Pfade für jedes gefundene Profil generieren
            foreach (var profile in profileInfos)
            {
                string fullProfilePathRoaming;
                string fullProfilePathLocal;

                if (profile.IsRelative)
                {
                    // Standardfall: Pfad ist relativ zu AppData/Mozilla/Firefox (z.B. "Profiles/abc.default")
                    fullProfilePathRoaming = _os.WindowsFileSystemService.CombinePaths(firefoxRoamingRoot, profile.Path);
                    fullProfilePathLocal = _os.WindowsFileSystemService.CombinePaths(firefoxLocalRoot, profile.Path);
                }
                else
                {
                    // Seltener Fall: Benutzer hat Profil an einem ganz anderen Ort gespeichert (absoluter Pfad)
                    fullProfilePathRoaming = profile.Path;

                    // Bei absoluten Pfaden ist der lokale Cache-Pfad schwerer zu erraten.
                    // Firefox nutzt oft einen Hash des Pfades in LocalAppData.
                    // Wir versuchen hier einen Best-Guess: LocalRoot + Ordnername des absoluten Pfads
                    var folderName = new DirectoryInfo(profile.Path).Name;
                    fullProfilePathLocal = _os.WindowsFileSystemService.CombinePaths(firefoxLocalRoot, "Profiles", folderName);
                }

                // --- CACHE (LocalAppData) ---
                // Pfad: ...\Profiles\xxxx.default\cache2\entries
                cacheDirs.Add(_os.WindowsFileSystemService.CombinePaths(fullProfilePathLocal, "cache2", "entries"));
                // Optional: Auch den StartupCache löschen
                cacheDirs.Add(_os.WindowsFileSystemService.CombinePaths(fullProfilePathLocal, "startupCache"));

                // --- COOKIES (Roaming) ---
                // Pfad: ...\Profiles\xxxx.default\storage\default
                // (Firefox speichert Daten für Webseiten hier in Unterordnern)
                cookiesDirs.Add(_os.WindowsFileSystemService.CombinePaths(fullProfilePathRoaming, "storage", "default"));

                // --- EXTENSIONS (Roaming) ---
                // Pfad: ...\Profiles\xxxx.default\extensions
                extensionsDirs.Add(_os.WindowsFileSystemService.CombinePaths(fullProfilePathRoaming, "extensions"));
            }

            return new BrowserPaths(cacheDirs, cookiesDirs, extensionsDirs);
        }

        // ---------------------------------------------------------------------------------
        // HELPER: profiles.ini Parsen
        // ---------------------------------------------------------------------------------

        private class ProfileInfo
        {
            public string Path { get; set; } = string.Empty;
            public bool IsRelative { get; set; } = true;
        }

        private List<ProfileInfo> GetProfileFoldersFromIni(string iniPath)
        {
            var profiles = new List<ProfileInfo>();

            if (!_os.WindowsFileSystemService.FileExists(iniPath)) return profiles;

            try
            {
                // Wir nutzen System.IO direkt für das zeilenweise Lesen
                var lines = File.ReadAllLines(iniPath);

                // Temporäre Variablen für den aktuellen Block
                string? currentPath = null;
                bool? currentIsRelative = null;

                foreach (var line in lines)
                {
                    // Ein neuer Abschnitt beginnt (z.B. [Profile0] oder [Install...])
                    if (line.Trim().StartsWith("[") && line.Trim().EndsWith("]"))
                    {
                        // Wenn wir vorher Daten gesammelt haben, speichern wir sie jetzt
                        if (currentPath != null)
                        {
                            profiles.Add(new ProfileInfo
                            {
                                Path = currentPath.Replace('/', '\\'), // Linux-Style Slashes in Windows Slashes wandeln
                                IsRelative = currentIsRelative ?? true
                            });
                        }

                        // Reset für neuen Block
                        currentPath = null;
                        currentIsRelative = null;
                        continue;
                    }

                    // Daten auslesen
                    if (line.StartsWith("Path=", StringComparison.OrdinalIgnoreCase))
                    {
                        currentPath = line.Substring(5).Trim();
                    }
                    else if (line.StartsWith("IsRelative=", StringComparison.OrdinalIgnoreCase))
                    {
                        // IsRelative=1 -> true, IsRelative=0 -> false
                        currentIsRelative = line.Trim().EndsWith("1");
                    }
                }

                // Den allerletzten Eintrag nicht vergessen (da keine neue Klammer [ mehr kommt)
                if (currentPath != null)
                {
                    profiles.Add(new ProfileInfo
                    {
                        Path = currentPath.Replace('/', '\\'),
                        IsRelative = currentIsRelative ?? true
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Parsen der profiles.ini");
            }

            return profiles;
        }

        // ---------------------------------------------------------------------------------
        // EXECUTABLE PATHS (Deine bestehende Logik)
        // ---------------------------------------------------------------------------------
        protected override List<string> ExecutablePaths
        {
            get
            {
                var paths = new List<string>();

                // 1. REGISTRY: Dynamic Lookup
                string? GetPathFromMozillaRegistry(bool isHklm)
                {
                    string rootKey = @"Software\Mozilla\Mozilla Firefox";

                    var currentVersionObj = isHklm
                        ? _os.WindowsRegistryService.GetLocalMachineValue(rootKey, "CurrentVersion")
                        : _os.WindowsRegistryService.GetCurrentUserValue(rootKey, "CurrentVersion");

                    if (currentVersionObj == null) return null;

                    string currentVersion = currentVersionObj.ToString()!;
                    string mainKeyPath = $@"{rootKey}\{currentVersion}\Main";

                    var pathToExeObj = isHklm
                        ? _os.WindowsRegistryService.GetLocalMachineValue(mainKeyPath, "PathToExe")
                        : _os.WindowsRegistryService.GetCurrentUserValue(mainKeyPath, "PathToExe");

                    return pathToExeObj?.ToString();
                }

                var hkcuPath = GetPathFromMozillaRegistry(false);
                if (!string.IsNullOrEmpty(hkcuPath)) paths.Add(hkcuPath);

                var hklmPath = GetPathFromMozillaRegistry(true);
                if (!string.IsNullOrEmpty(hklmPath)) paths.Add(hklmPath);

                // 2. STANDARDPFADE (Fallback)
                paths.Add(_os.WindowsFileSystemService.CombinePaths(_os.WindowsFileSystemService.GetEnvironmentPath("ProgramFiles"), @"Mozilla Firefox\firefox.exe"));
                paths.Add(_os.WindowsFileSystemService.CombinePaths(_os.WindowsFileSystemService.GetEnvironmentPath("ProgramFiles(x86)"), @"Mozilla Firefox\firefox.exe"));
                paths.Add(_os.WindowsFileSystemService.CombinePaths(_os.WindowsFileSystemService.GetEnvironmentPath("LocalAppData"), @"Mozilla Firefox\firefox.exe"));

                return paths.Distinct().ToList();
            }
        }
    }

    //public class FirefoxBrowser : BrowserBase
    //{// Konstanten (Könnten auch aus deiner WebLinks Klasse kommen)
    //    private const string FirefoxEbesucherAddOnLink = "https://addons.mozilla.org/de/firefox/addon/ebesucher-addon1/";
    //    private const string EbesucherAddOnNameForFirefox = "{fef425dc-a60f-4484-954d-71ecf2544846}.xpi";
    //    public override string DisplayName => "Firefox";
    //    public override string IconPath => "ms-appx:///Resources/Visuals/Icons/Intersection/fa_firefox.png";
    //    public override string DownloadUrl => WebLinks.FirefoxDownloadLink; // Stellen Sie sicher, dass WebLinks existiert
    //    // --- Implementierung der neuen abstrakten Properties ---
    //    public override string ExtensionInstallUrl => FirefoxEbesucherAddOnLink;
    //    public override BrowserType Type => BrowserType.Firefox;
    //    protected override string ProcessName => "firefox";

    //    // Basis-Pfad ohne Version für spätere Versions-Checks
    //    protected override string RegistryKeyVersion => @"Software\Mozilla\Mozilla Firefox";

    //    public FirefoxBrowser(IOperatingSystemFacade os, ILogger<FirefoxBrowser> logger)
    //        : base(os, logger) { }

    //    // --- Implementierung der Extension-Prüfung ---
    //    public override bool IsExtensionInstalled(string? extensionId = null)
    //    {
    //        var paths = GetPaths();
    //        var extensionsDir = paths.ExtensionsDir;

    //        // Firefox Extensions liegen im Profilordner unter "extensions"
    //        if (!_os.WindowsFileSystemService.DirectoryExists(extensionsDir)) return false;

    //        // Wenn keine spezifische ID übergeben wird, nehmen wir unseren Standard-Namen
    //        // Wir entfernen führende Backslashes, falls in der Konstante vorhanden, um sauber zu kombinieren
    //        var idToCheck = string.IsNullOrEmpty(extensionId)
    //            ? EbesucherAddOnNameForFirefox.TrimStart('\\')
    //            : extensionId;

    //        // Fall 1: Die Extension ist eine .xpi Datei (häufigster Fall)
    //        var xpiPath = _os.WindowsFileSystemService.CombinePaths(extensionsDir, idToCheck);
    //        if (_os.WindowsFileSystemService.FileExists(xpiPath)) return true;

    //        // Fall 2: Die Extension ist ein entpackter Ordner (z.B. bei Sideloading oder Entwicklung)
    //        // Dafür entfernen wir ".xpi" vom Namen, falls vorhanden
    //        var folderName = idToCheck.Replace(".xpi", "");
    //        var folderPath = _os.WindowsFileSystemService.CombinePaths(extensionsDir, folderName);

    //        return _os.WindowsFileSystemService.DirectoryExists(folderPath);
    //    }

    //    protected override List<string> ExecutablePaths
    //    {
    //        get
    //        {
    //            var paths = new List<string>();

    //            // ---------------------------------------------------------
    //            // 1. REGISTRY: Dynamic Lookup (CurrentVersion -> Main -> PathToExe)
    //            // ---------------------------------------------------------

    //            // Lokale Hilfsfunktion, um den zweistufigen Lookup zu machen
    //            string? GetPathFromMozillaRegistry(bool isHklm)
    //            {
    //                // Schritt A: Den Wert "CurrentVersion" auslesen
    //                // Pfad: Software\Mozilla\Mozilla Firefox
    //                string rootKey = @"Software\Mozilla\Mozilla Firefox";

    //                var currentVersionObj = isHklm
    //                    ? _os.WindowsRegistryService.GetLocalMachineValue(rootKey, "CurrentVersion")
    //                    : _os.WindowsRegistryService.GetCurrentUserValue(rootKey, "CurrentVersion");

    //                if (currentVersionObj == null) return null;

    //                string currentVersion = currentVersionObj.ToString()!;
    //                // Ergebnis z.B.: "146.0.1 (x64 de)"

    //                // Schritt B: Den Pfad zum "Main" Schlüssel bauen
    //                // Pfad: Software\Mozilla\Mozilla Firefox\146.0.1 (x64 de)\Main
    //                string mainKeyPath = $@"{rootKey}\{currentVersion}\Main";

    //                // Schritt C: Den Wert "PathToExe" auslesen
    //                var pathToExeObj = isHklm
    //                    ? _os.WindowsRegistryService.GetLocalMachineValue(mainKeyPath, "PathToExe")
    //                    : _os.WindowsRegistryService.GetCurrentUserValue(mainKeyPath, "PathToExe");

    //                return pathToExeObj?.ToString();
    //            }

    //            // Suche in HKCU
    //            var hkcuPath = GetPathFromMozillaRegistry(false);
    //            if (!string.IsNullOrEmpty(hkcuPath)) paths.Add(hkcuPath);

    //            // Suche in HKLM (Falls als Admin für alle User installiert)
    //            var hklmPath = GetPathFromMozillaRegistry(true);
    //            if (!string.IsNullOrEmpty(hklmPath)) paths.Add(hklmPath);


    //            // ---------------------------------------------------------
    //            // 2. STANDARDPFADE (Fallback)
    //            // ---------------------------------------------------------
    //            paths.Add(_os.WindowsFileSystemService.CombinePaths(_os.WindowsFileSystemService.GetEnvironmentPath("ProgramFiles"), @"Mozilla Firefox\firefox.exe"));
    //            paths.Add(_os.WindowsFileSystemService.CombinePaths(_os.WindowsFileSystemService.GetEnvironmentPath("ProgramFiles(x86)"), @"Mozilla Firefox\firefox.exe"));

    //            // Firefox kann auch im AppData liegen (User-Install ohne Admin)
    //            paths.Add(_os.WindowsFileSystemService.CombinePaths(_os.WindowsFileSystemService.GetEnvironmentPath("LocalAppData"), @"Mozilla Firefox\firefox.exe"));

    //            return [.. paths.Distinct()];
    //        }
    //    }

    //    public override BrowserPaths GetPaths()
    //    {
    //        var appData = _os.WindowsFileSystemService.GetEnvironmentPath("AppData"); // Roaming!
    //        var firefoxRoot = _os.WindowsFileSystemService.CombinePaths(appData, "Mozilla", "Firefox");

    //        // Profilname ermitteln
    //        var profileName = GetProfileNameFromIni(_os.WindowsFileSystemService.CombinePaths(firefoxRoot, "profiles.ini"));

    //        var profilePathRoaming = _os.WindowsFileSystemService.CombinePaths(firefoxRoot, "Profiles", profileName);
    //        var profilePathLocal = _os.WindowsFileSystemService.CombinePaths(_os.WindowsFileSystemService.GetEnvironmentPath("LocalAppData"), "Mozilla", "Firefox", "Profiles", profileName);

    //        return new BrowserPaths(
    //            CacheDir: _os.WindowsFileSystemService.CombinePaths(profilePathLocal, "cache2", "entries"),
    //            CookiesDir: _os.WindowsFileSystemService.CombinePaths(profilePathRoaming, "storage", "default"),
    //            ExtensionsDir: _os.WindowsFileSystemService.CombinePaths(profilePathRoaming, "extensions")
    //        );
    //    }

    //    private string GetProfileNameFromIni(string iniPath)
    //    {
    //        // Einfacher Fallback, falls Datei nicht existiert oder Parsing zu komplex für diesen Ausschnitt ist
    //        if (!_os.WindowsFileSystemService.FileExists(iniPath)) return "Default";

    //        // Hinweis: Hier müsste eine Logik rein, die die profiles.ini parst, 
    //        // nach "Path=Profiles/xxxx.default" sucht und das "Profiles/" abschneidet.
    //        // Für jetzt geben wir einen Platzhalter zurück oder man müsste IFileSystemService um ReadAllLines erweitern.

    //        return "Default";
    //    }
    //}
}
