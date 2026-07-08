using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.UseCases;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Update;

namespace eBRestarter.Core.Application.UseCases;

public sealed class ManageApplicationUpdatesUseCase(IOutboundPortUpdate updateService) : IUseCaseManageApplicationUpdates
{
    private readonly IOutboundPortUpdate _updateService = updateService;

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

