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
