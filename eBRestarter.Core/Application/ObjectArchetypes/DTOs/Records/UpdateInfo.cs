namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

public sealed record UpdateInfo
{
    public bool IsUpdateAvailable { get; init; }
    public string LatestVersion { get; init; } = string.Empty;
    public string CurrentVersion { get; init; } = string.Empty;
    public string DownloadUrl { get; init; } = string.Empty;
    public string Changelog { get; init; } = string.Empty;
    public DateTime PublishedAt { get; init; }
}
