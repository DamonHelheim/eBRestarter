using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models.Records;

namespace eBRestarter.Core.Application.Ports.Outbound.Browser;

/// <summary>
/// Defines the capabilities and properties a browser service must provide.
/// </summary>
public interface IBrowserPort
{
    string DisplayName { get; }
    string IconPath { get; }
    string DownloadUrl { get; }

    BrowserType Type { get; }
    string ProcessName { get; }
    string BrowserVersion { get; }
    bool IsInstalled { get; }
    string ExtensionInstallUrl { get; }

    void Start(string url, string arguments = "");
    void Close();

    /// <summary>
    /// Resolves cache and path information.
    /// </summary>
    BrowserPaths ResolvePaths();

    bool IsExtensionInstalled(string? extensionId = null);
}


