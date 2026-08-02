using System;
using System.IO;
using System.Runtime.Versioning;

using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Application;

namespace eBRestarter.Infrastructure.BehavioralComponents.Providers;

/// <summary>
/// Provider Component: Infrastructure provider determining OS-specific application directory and file paths.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class WindowsAppPathProvider(
    IOutboundPortAppPathProvider pathProvider)
    : IInboundPortOsAppPathProvider
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════

    // ── Block 2: Primitive Typen & Strings (alphabetisch) ──
    private const string AppFolderName = "eBRestarter";
    private const string ConfigFileName = "eBRestarterConfig.json";
    private const string DownloadsFolderName = "Downloads";
    private const string LogFileName = "log.txt";
    private const string SkylarFolderName = "Skylar";


    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════

    // ── Block 1: Injizierte Abhängigkeiten (alphabetisch) ──
    private readonly IOutboundPortAppPathProvider _pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════

    public string RetrieveAppDataPath()
    {
        var localAppData = _pathProvider.RetrieveLocalAppDataDirectory();
        return Path.Combine(localAppData, SkylarFolderName, AppFolderName);
    }

    public string RetrieveConfigFilePath() => Path.Combine(RetrieveAppDataPath(), ConfigFileName);

    public string RetrieveDownloadsPath() => Path.Combine(_pathProvider.RetrieveUserProfileDirectory(), DownloadsFolderName);

    public string RetrieveLogFilePath() => Path.Combine(RetrieveAppDataPath(), LogFileName);
}
