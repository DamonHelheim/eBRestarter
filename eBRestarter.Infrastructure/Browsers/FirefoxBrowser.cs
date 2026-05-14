using eBRestarter.Core.Application.Constants;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Infrastructure.Browsers.Abstract;
using Microsoft.Extensions.Logging;

namespace eBRestarter.Infrastructure.Browsers;

public class FirefoxBrowser(IOperatingSystemFacade os, ILogger<FirefoxBrowser> logger) : BrowserBase(os, logger)
{
    private const string EbesucherAddOnNameForFirefox = "{76e6445a-74a5-4c26-9afc-95dae514cb77}.xpi";

    public override string DisplayName => "Firefox";
    public override string IconPath => "ms-appx:///Resources/Visuals/Icons/Intersection/fa_firefox.png";
    public override string DownloadUrl => WebLinks.FirefoxDownloadLink;
    public override string ExtensionInstallUrl => WebLinks.FirefoxEVisitorAddOnLink;
    public override BrowserType Type => BrowserType.Firefox;

    protected override List<string> ExecutablePaths
    {
        get
        {
            var paths = new List<string>();

            // 1. REGISTRY: Dynamic Lookup
            var hkcuPath = RetrievePathFromMozillaRegistry(false);

            if (!string.IsNullOrEmpty(hkcuPath))
            {

                paths.Add(hkcuPath);
            }

            var hklmPath = RetrievePathFromMozillaRegistry(true);

            if (!string.IsNullOrEmpty(hklmPath))
            {
                paths.Add(hklmPath);
            }

            // 2. STANDARDPFADE (Fallback)
            paths.Add(_os.WindowsFileSystemService.CombinePaths(_os.WindowsFileSystemService.ResolveEnvironmentPath("ProgramFiles"), @"Mozilla Firefox\firefox.exe"));
            paths.Add(_os.WindowsFileSystemService.CombinePaths(_os.WindowsFileSystemService.ResolveEnvironmentPath("ProgramFiles(x86)"), @"Mozilla Firefox\firefox.exe"));
            paths.Add(_os.WindowsFileSystemService.CombinePaths(_os.WindowsFileSystemService.ResolveEnvironmentPath("LocalAppData"), @"Mozilla Firefox\firefox.exe"));

            return [.. paths.Distinct()];

            string? RetrievePathFromMozillaRegistry(bool isHklm)
            {
                const string rootKey = @"Software\Mozilla\Mozilla Firefox";

                var currentVersionObj = isHklm
                    ? _os.WindowsRegistryService.RetrieveLocalMachineValue(rootKey, "CurrentVersion")
                    : _os.WindowsRegistryService.RetrieveCurrentUserValue(rootKey, "CurrentVersion");

                if (currentVersionObj is null) return null;

                string currentVersion = currentVersionObj.ToString()!;
                string mainKeyPath = $@"{rootKey}\{currentVersion}\Main";

                var pathToExeObj = isHklm
                    ? _os.WindowsRegistryService.RetrieveLocalMachineValue(mainKeyPath, "PathToExe")
                    : _os.WindowsRegistryService.RetrieveCurrentUserValue(mainKeyPath, "PathToExe");

                return pathToExeObj?.ToString();
            }
        }
    }

    public override string ProcessName => "firefox";
    protected override string RegistryKeyVersion => @"Software\Mozilla\Mozilla Firefox";

    // PFADE: Cache, Cookies & Extensions für ALLE Profile ermitteln
    public override BrowserPaths ResolvePaths()
    {
        var appData = _os.WindowsFileSystemService.ResolveEnvironmentPath("AppData"); // Roaming
        var localAppData = _os.WindowsFileSystemService.ResolveEnvironmentPath("LocalAppData"); // Local

        var firefoxRoamingRoot = _os.WindowsFileSystemService.CombinePaths(appData, "Mozilla", "Firefox");
        var firefoxLocalRoot = _os.WindowsFileSystemService.CombinePaths(localAppData, "Mozilla", "Firefox");

        // Listen für die Ergebnisse
        var cacheDirs = new List<string>();
        var cookiesDirs = new List<string>();
        var extensionsDirs = new List<string>();

        // 1. profiles.ini einlesen und alle Profil-Ordner finden
        var profilesIniPath = _os.WindowsFileSystemService.CombinePaths(firefoxRoamingRoot, "profiles.ini");
        var profileInfos = RetrieveProfileFoldersFromIni(profilesIniPath);

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
            // Pfad: ...\Profiles\xxxx.default\cache2\entries
            cacheDirs.Add(_os.WindowsFileSystemService.CombinePaths(fullProfilePathLocal, "cache2", "entries"));

            // Optional: Auch den StartupCache löschen
            cacheDirs.Add(_os.WindowsFileSystemService.CombinePaths(fullProfilePathLocal, "startupCache"));

            // Pfad: ...\Profiles\xxxx.default\storage\default
            // (Firefox speichert Daten für Webseiten hier in Unterordnern)
            cookiesDirs.Add(_os.WindowsFileSystemService.CombinePaths(fullProfilePathRoaming, "storage", "default"));

            // Pfad: ...\Profiles\xxxx.default\extensions
            extensionsDirs.Add(_os.WindowsFileSystemService.CombinePaths(fullProfilePathRoaming, "extensions"));
        }

        return new BrowserPaths(cacheDirs, cookiesDirs, extensionsDirs);
    }

    // PRÜFUNG: Ist die Extension installiert? (Sucht in ALLEN Profilen)
    public override bool IsExtensionInstalled(string? extensionId = null)
    {
        var paths = ResolvePaths();

        // Wenn keine Extensions-Ordner gefunden wurden, abbrechen
        if (paths.ExtensionsDirs is null || paths.ExtensionsDirs.Count == 0) return false;

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

    private List<ProfileInfo> RetrieveProfileFoldersFromIni(string iniPath)
    {
        var profiles = new List<ProfileInfo>();

        if (!_os.WindowsFileSystemService.FileExists(iniPath)) return profiles;

        try
        {
            // Wir nutzen System.IO direkt für das zeilenweise Lesen
            var lines = _os.WindowsFileSystemService.ReadAllLines(iniPath);

            // Temporäre Variablen für den aktuellen Block
            string? currentPath = null;
            bool? currentIsRelative = null;

            foreach (var line in lines)
            {
                // Ein neuer Abschnitt beginnt (z.B. [Profile0] oder [Install...])
                if (line.Trim().StartsWith('[') && line.Trim().EndsWith(']'))
                {
                    // Wenn wir vorher Daten gesammelt haben, speichern wir sie jetzt
                    if (currentPath is not null)
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
                    currentPath = line[5..].Trim();
                }
                else if (line.StartsWith("IsRelative=", StringComparison.OrdinalIgnoreCase))
                {
                    // IsRelative=1 -> true, IsRelative=0 -> false
                    currentIsRelative = line.Trim().EndsWith('1');
                }
            }

            // Den allerletzten Eintrag nicht vergessen (da keine neue Klammer [ mehr kommt)
            if (currentPath is not null)
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

    private sealed class ProfileInfo
    {
        public string Path { get; set; } = string.Empty;
        public bool IsRelative { get; set; } = true;
    }
}

//Cookie source

//C:\Users\Workstation\AppData\Roaming\Mozilla\Firefox\Profiles\opng5oi1.default-release\storage\default => Cookies
//"C:\Users\Workstation\AppData\Roaming\Mozilla\Firefox\Profiles\opng5oi1.default-release\cookies.sqlite"


//Internetcache source
//C:\Users\Workstation\AppData\Local\Mozilla\Firefox\Profiles\opng5oi1.default-release\cache2\doomed
//C:\Users\Workstation\AppData\Local\Mozilla\Firefox\Profiles\opng5oi1.default-release\cache2\entries
//C:\Users\Workstation\AppData\Local\Mozilla\Firefox\Profiles\opng5oi1.default-release\jumpListCache

//Current open tabs
//C:\Users\Workstation\AppData\Roaming\Mozilla\Firefox\Profiles\opng5oi1.default-release\sessionstore-backups

//C:\Users\Workstation\AppData\Roaming\Mozilla\Firefox\Profiles\opng5oi1.default-release\datareporting
// HELPER: profiles.ini Parsen



