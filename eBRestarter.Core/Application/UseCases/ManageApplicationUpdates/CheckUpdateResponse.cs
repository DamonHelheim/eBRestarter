namespace eBRestarter.Core.Application.UseCases.ManageApplicationUpdates;

public record CheckUpdateResponse(bool IsUpdateAvailable, string LatestVersion);
