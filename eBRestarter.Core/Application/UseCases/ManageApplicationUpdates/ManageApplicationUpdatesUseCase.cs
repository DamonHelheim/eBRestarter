using eBRestarter.Core.Application.Ports.Inbound.UseCases.ManageApplicationUpdates;
using eBRestarter.Core.Application.Ports.Outbound.Update;
using eBRestarter.Core.Application.Models.Records;

namespace eBRestarter.Core.Application.UseCases.ManageApplicationUpdates;

public sealed class ManageApplicationUpdatesUseCase(IUpdatePort updateService) : IManageApplicationUpdatesUseCase
{
    private readonly IUpdatePort _updateService = updateService;

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

