using eBRestarter.Core.Application.Ports.Inbound.Providers;
using eBRestarter.Core.Application.Ports.Outbound.Providers;
using eBRestarter.Core.Application.Ports.Outbound.Application;
using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using eBRestarter.Core.Application.Ports.Outbound.SystemInfo;
using System.Runtime.Versioning;

namespace eBRestarter.Core.Application.Providers;

[SupportedOSPlatform("windows")]
public sealed class WindowsOsPathProvider(IAppPathPort pathProvider) : IOsPathProviderPort
{
    private readonly IAppPathPort _pathProvider = pathProvider;

    private const string AppFolderName = "eBRestarter";
    private const string ConfigFileName = "eBRestarterConfig.json"; // Migrated to JSON!

    // %LocalAppData%/Skylar/eBRestarter/
    public string RetrieveAppDataPath()
    {
        string localAppData = _pathProvider.RetrieveLocalAppDataDirectory();

        // Optional "Skylar" parent directory included for legacy compatibility with previous implementations
        return Path.Combine(localAppData, "Skylar", AppFolderName);
    }

    public string RetrieveDownloadsPath()
    {
        // Standard Windows strategy to locate the user's local Downloads folder
        return Path.Combine(_pathProvider.RetrieveUserProfileDirectory(), "Downloads");
    }

    public string RetrieveConfigFilePath()
    {
        return Path.Combine(RetrieveAppDataPath(), ConfigFileName);
    }

    public string RetrieveLogFilePath()
    {
        return Path.Combine(RetrieveAppDataPath(), "log.txt");
    }
}









