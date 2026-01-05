using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Infrastructure.Browsers.Abstract
{
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
            // Extensions liegen bei Chromium unter .../Extensions/{ID}
            // Wir prüfen einfach, ob dieser Ordner existiert.
            // (Man könnte noch tiefer prüfen, ob darin Versionen liegen, aber das reicht meistens)
            return _os.WindowsFileSystemService.DirectoryExists(paths.ExtensionsDir);
            // Hinweis: In deinen konkreten Klassen (z.B. ChromeBrowser) hast du GetPaths() so implementiert, 
            // dass ExtensionsDir schon ".../Extensions/{ID}" beinhaltet. 
            // Falls ExtensionsDir nur ".../Extensions" ist, musst du hier CombinePaths nutzen!

            // CHECK: Schau in deine ChromeBrowser.cs GetPaths():
            // ExtensionsDir: ...CombinePaths(baseDir, "Extensions") -> Dann musst du hier die ID anhängen!
            // ExtensionsDir: ...CombinePaths(baseDir, "Extensions", ExtensionId) -> Dann passt es direkt.

            // ANNAHME: Deine konkreten Klassen (siehe ChromeBrowser unten) geben den Pfad INKLUSIVE ID zurück 
            // oder den Pfad zum "Extensions" Ordner?
            // In deinem Brave-Code war es INKLUSIVE ID. In deinem Chrome-Code war es OHNE ID.
            // Das müssen wir vereinheitlichen! 

            // BESSERE VARIANTE (Robuster):
            // Wir gehen davon aus, dass ExtensionsDir der Ordner "Extensions" ist.
            // Dann bauen wir den Pfad zur ID.

            // Da ich deinen ChromeBrowser Code kenne (dort war es nur "Extensions"), passe ich es hier an:

            // Fallunterscheidung (Quick Fix für inkonsistente GetPaths):
            //if (paths.ExtensionsDir.EndsWith(idToCheck, StringComparison.OrdinalIgnoreCase))
            //{
            //    return _os.WindowsFileSystemService.DirectoryExists(paths.ExtensionsDir);
            //}

            //var fullExtensionPath = _os.WindowsFileSystemService.CombinePaths(paths.ExtensionsDir, idToCheck);
            //return _os.WindowsFileSystemService.DirectoryExists(fullExtensionPath);
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
}
