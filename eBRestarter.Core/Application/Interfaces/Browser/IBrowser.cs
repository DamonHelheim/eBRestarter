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
    string BrowserVersion { get; }
    bool IsInstalled { get; }
    void Start(string url, string arguments = "");
    void Close();

    // Cache & Pfad Infos
    BrowserPaths GetPaths();

    bool IsExtensionInstalled(string extensionId); // NEU
    string ExtensionInstallUrl { get; } // NEU: Link zum Store


}
