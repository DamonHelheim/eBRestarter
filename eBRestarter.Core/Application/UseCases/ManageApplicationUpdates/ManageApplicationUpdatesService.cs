using eBRestarter.Core.Application.Interfaces.Update;

namespace eBRestarter.Core.Application.UseCases.ManageApplicationUpdates;

public class ManageApplicationUpdatesService(IUpdateService updateService) : IManageApplicationUpdatesUseCase
{
    private readonly IUpdateService _updateService = updateService;

    public async Task<CheckUpdateResponse> CheckForUpdatesAsync()
    {
        var updateInfo = await _updateService.CheckForUpdateAsync();
        return new CheckUpdateResponse(updateInfo.IsUpdateAvailable, updateInfo.LatestVersion);
    }

    public async Task PerformUpdateAsync()
    {
        var updateInfo = await _updateService.CheckForUpdateAsync();
        if (updateInfo.IsUpdateAvailable)
        {
            await _updateService.DownloadAndInstallAsync(updateInfo);
        }
    }
}
