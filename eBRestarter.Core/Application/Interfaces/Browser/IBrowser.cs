using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models.Records;

namespace eBRestarter.Core.Application.Interfaces.Browser;

//(Was muss ein Browser können?)
public interface IBrowser
{
    string DisplayName { get; }
    string IconPath { get; }
    string DownloadUrl { get; }

    BrowserType Type { get; }
    string ProcessName { get; }
    string BrowserVersion { get; }
    bool IsInstalled { get; }
    string ExtensionInstallUrl { get; } // NEU: Link zum Store

    void Start(string url, string arguments = "");
    void Close();

    // Cache & Pfad Infos
    BrowserPaths ResolvePaths();

    bool IsExtensionInstalled(string? extensionId = null); // NEU
}
