namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

public sealed record CheckUpdateResponse(bool IsUpdateAvailable, string LatestVersion);

