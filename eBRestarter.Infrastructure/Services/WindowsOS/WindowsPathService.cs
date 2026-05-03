using eBRestarter.Core.Application.Interfaces;
using System.Runtime.Versioning;

namespace eBRestarter.Infrastructure.Services.WindowsOS;

[SupportedOSPlatform("windows")]
public class WindowsPathService(IPathProvider pathProvider) : IPathService
{
    private readonly IPathProvider _pathProvider = pathProvider;

    private const string AppFolderName = "eBRestarter";
    private const string ConfigFileName = "eBRestarterConfig.json"; // Jetzt JSON!

    // %LocalAppData%/eBRestarter/
    public string GetAppDataPath()
    {
        string localAppData = _pathProvider.GetLocalAppDataDirectory();
        // "Skylar" Ordner optional dazwischen, wie in deinem alten Code
        return Path.Combine(localAppData, "Skylar", AppFolderName);
    }

    public string GetDownloadsPath()
    {
        // Windows-Weg um den Downloads Ordner zu finden
        return Path.Combine(_pathProvider.GetUserProfileDirectory(), "Downloads");
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
