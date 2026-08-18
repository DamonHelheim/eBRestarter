namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

/// <summary>
/// Encapsulates application update details and release metadata.
/// </summary>
public sealed record UpdateInfo
{
    /// <summary>
    /// Gets a value indicating whether an update is available.
    /// </summary>
    public bool IsUpdateAvailable { get; init; }

    /// <summary>
    /// Gets the version identifier of the latest update.
    /// </summary>
    public string LatestVersion { get; init; } = string.Empty;

    /// <summary>
    /// Gets the version identifier of the current installation.
    /// </summary>
    public string CurrentVersion { get; init; } = string.Empty;

    /// <summary>
    /// Gets the download URL for the update package.
    /// </summary>
    public string DownloadUrl { get; init; } = string.Empty;

    /// <summary>
    /// Gets the release notes or changelog content.
    /// </summary>
    public string Changelog { get; init; } = string.Empty;

    /// <summary>
    /// Gets the publication date and time of the release.
    /// </summary>
    public DateTime PublishedAt { get; init; }
}
