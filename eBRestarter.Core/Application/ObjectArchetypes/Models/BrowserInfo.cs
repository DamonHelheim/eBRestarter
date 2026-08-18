using eBRestarter.Core.Application.ObjectArchetypes.Enums;

namespace eBRestarter.Core.Application.ObjectArchetypes.Models;

/// <summary>
/// Represents metadata and installation status of a supported web browser.
/// </summary>
public sealed class BrowserInfo
{
    /// <summary>
    /// Gets or sets the strongly typed browser application type.
    /// </summary>
    public BrowserType Type { get; set; }

    /// <summary>
    /// Gets or sets the human-readable display name of the browser.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detected version string of the browser installation.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the browser is installed on the host system.
    /// </summary>
    public bool IsInstalled { get; set; }

    /// <summary>
    /// Gets or sets the file system or asset path to the browser icon.
    /// </summary>
    public string IconPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the official download URL for installing the browser.
    /// </summary>
    public string DownloadUrl { get; set; } = string.Empty;
}
