namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

/// <summary>
/// Represents the result of an application update availability check.
/// </summary>
/// <param name="IsUpdateAvailable">Indicates whether a newer application version is available.</param>
/// <param name="LatestVersion">The version identifier of the latest available update.</param>
public sealed record CheckUpdateResponse(bool IsUpdateAvailable, string LatestVersion);

