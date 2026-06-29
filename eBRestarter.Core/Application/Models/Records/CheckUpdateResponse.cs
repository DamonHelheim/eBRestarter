namespace eBRestarter.Core.Application.Models.Records;

public sealed record CheckUpdateResponse(bool IsUpdateAvailable, string LatestVersion);

