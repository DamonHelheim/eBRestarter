using System;
using System.Threading.Tasks;

using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.UseCases;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Update;

namespace eBRestarter.Core.Application.UseCases;

/// <summary>
/// Use case implementation for checking application update status and executing automatic updates.
/// </summary>
/// <param name="updateService">Outbound port for update checking and installation.</param>
public sealed class ManageApplicationUpdatesUseCase(
    IOutboundPortUpdate updateService)
    : IUseCaseManageApplicationUpdates
{
    private readonly IOutboundPortUpdate _updateService = updateService ?? throw new ArgumentNullException(nameof(updateService));

    /// <inheritdoc />
    public async Task<CheckUpdateResponse> CheckForUpdatesAsync()
    {
        var updateInfo = await _updateService.CheckForUpdateAsync().ConfigureAwait(false);
        return new CheckUpdateResponse(updateInfo.IsUpdateAvailable, updateInfo.LatestVersion);
    }

    /// <inheritdoc />
    public async Task PerformUpdateAsync()
    {
        var updateInfo = await _updateService.CheckForUpdateAsync().ConfigureAwait(false);
        
        if (!updateInfo.IsUpdateAvailable)
        {
            return;
        }

        await _updateService.DownloadAndInstallAsync(updateInfo).ConfigureAwait(false);
    }
}
