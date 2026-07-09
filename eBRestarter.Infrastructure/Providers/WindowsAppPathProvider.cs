using System.Runtime.Versioning;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Application;

namespace eBRestarter.Infrastructure.Providers;

[SupportedOSPlatform("windows")]
public sealed class WindowsAppPathProvider(IOutboundPortAppPathProvider pathProvider) : IInboundPortOsAppPathProvider
{
    private readonly IOutboundPortAppPathProvider _pathProvider = pathProvider;

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









