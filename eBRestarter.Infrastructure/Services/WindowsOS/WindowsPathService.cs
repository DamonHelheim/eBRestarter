using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Text;

namespace eBRestarter.Infrastructure.Services.WindowsOS
{
    [SupportedOSPlatform("windows")]
    public class WindowsPathService : IPathService
    {
        private const string AppFolderName = "eBRestarter";
        private const string ConfigFileName = "eBRestarterConfig.json"; // Jetzt JSON!

        // %LocalAppData%/eBRestarter/
        public string GetAppDataPath()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            // "Skylar" Ordner optional dazwischen, wie in deinem alten Code
            return Path.Combine(localAppData, "Skylar", AppFolderName);
        }

        public string GetDownloadsPath()
        {
            // Windows-Weg um den Downloads Ordner zu finden
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        }

        public string GetConfigFilePath()
        {
            return Path.Combine(GetAppDataPath(), ConfigFileName);
        }

        public string GetLogFilePath()
        {
            return Path.Combine(GetAppDataPath(), "log.txt");
        }
    }
}
