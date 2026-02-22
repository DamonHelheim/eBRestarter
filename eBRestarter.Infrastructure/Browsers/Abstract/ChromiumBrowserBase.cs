using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using Microsoft.Extensions.Logging;

namespace eBRestarter.Infrastructure.Browsers.Abstract;
public abstract class ChromiumBrowserBase : BrowserBase
{
    protected ChromiumBrowserBase(IOperatingSystemFacade os, ILogger logger) : base(os, logger) { }

    // Diese Werte müssen die konkreten Klassen liefern
    protected abstract string ExeFileName { get; }       // e. g. "chrome.exe"
    protected abstract string BrowserRegistryName { get; } // e.g. "Google Chrome"
    protected abstract string UninstallSubKey { get; }   // e.g. "Google Chrome" oder "BraveSoftware Brave-Browser"
    protected abstract string ProgramFilesSubPath { get; } // e.g. @"Google\Chrome\Application"

    // Muss von Chrome/Edge/Brave implementiert werden
    protected abstract string ExtensionId { get; }

    // --- Implementierung der abstrakten Methode aus BrowserBase ---
    public override bool IsExtensionInstalled(string? extensionId = null)
    {
        // Wenn keine spezifische ID übergeben wurde, nimm die Standard-ID des Browsers
        var idToCheck = string.IsNullOrEmpty(extensionId) ? ExtensionId : extensionId;

        if (string.IsNullOrEmpty(idToCheck)) return false;

        var paths = GetPaths();

        // Wenn keine Extensions-Ordner gefunden wurden (z.B. keine Profile), ist auch nix installiert
        if (paths.ExtensionsDirs == null || paths.ExtensionsDirs.Count == 0) return false;

        // Wir prüfen JEDEN gefundenen Profil-Extensions-Ordner
        foreach (var extensionsDir in paths.ExtensionsDirs)
        {
            if (!_os.WindowsFileSystemService.DirectoryExists(extensionsDir)) continue;

            // Logik für Inkonsistenz-Behebung:
            // Manche Subklassen (z.B. Brave) geben Pfad INKL. ID zurück.
            // Andere (z.B. Chrome/Edge, wenn korrigiert) geben nur den ".../Extensions" Ordner zurück.

            // Fall A: Der Pfad endet bereits auf die ID (Brave-Style)
            if (extensionsDir.EndsWith(idToCheck, StringComparison.OrdinalIgnoreCase))
            {
                // Der Ordner existiert ja schon (oben geprüft), also ist sie da.
                // (Optional könnte man noch prüfen, ob Version-Unterordner drin sind)
                return true;
            }

            // Fall B: Der Pfad ist nur der "Extensions"-Ordner -> Wir müssen die ID anhängen
            var fullExtensionPath = _os.WindowsFileSystemService.CombinePaths(extensionsDir, idToCheck);
            if (_os.WindowsFileSystemService.DirectoryExists(fullExtensionPath))
            {
                return true;
            }
        }

        return false;
    }

    protected override List<string> ExecutablePaths
    {
        get
        {
            var paths = new List<string>();

            // 1. App Paths
            var appPathKey = $@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\{ExeFileName}";
            var appPath = _os.WindowsRegistryService.GetLocalMachineValue(appPathKey, "")?.ToString();
            if (!string.IsNullOrEmpty(appPath)) paths.Add(appPath);

            // 2. StartMenuInternet
            var clientKey = $@"SOFTWARE\Clients\StartMenuInternet\{BrowserRegistryName}\shell\open\command";
            var clientPath = _os.WindowsRegistryService.GetLocalMachineValue(clientKey, "")?.ToString();
            if (!string.IsNullOrEmpty(clientPath)) paths.Add(clientPath.Replace("\"", "").Trim());

            // 3. Uninstall Keys (Hier nutzen wir die generische Logik)
            AddPathFromUninstallKey(paths, $@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{UninstallSubKey}", ExeFileName, true);
            AddPathFromUninstallKey(paths, $@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{UninstallSubKey}", ExeFileName, true);
            AddPathFromUninstallKey(paths, $@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{UninstallSubKey}", ExeFileName, false);

            // 4. Standardpfade
            var pathPart = _os.WindowsFileSystemService.CombinePaths(ProgramFilesSubPath, ExeFileName);

            paths.Add(_os.WindowsFileSystemService.CombinePaths(_os.WindowsFileSystemService.GetEnvironmentPath("ProgramFiles"), pathPart));
            paths.Add(_os.WindowsFileSystemService.CombinePaths(_os.WindowsFileSystemService.GetEnvironmentPath("ProgramFiles(x86)"), pathPart));
            paths.Add(_os.WindowsFileSystemService.CombinePaths(_os.WindowsFileSystemService.GetEnvironmentPath("LocalAppData"), pathPart));

            return paths.Distinct().ToList();
        }
    }
}
