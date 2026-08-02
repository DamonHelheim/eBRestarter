using System;
using System.Threading.Tasks;

using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.UseCases;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Update;

namespace eBRestarter.Core.Application.UseCases;

/// <summary>
/// Use case implementation for checking application update status and executing automatic updates.
/// </summary>
public sealed class ManageApplicationUpdatesUseCase(
    IOutboundPortUpdate updateService)
    : IUseCaseManageApplicationUpdates
{
    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════

    // ── Block 1: Injizierte Abhängigkeiten (alphabetisch A–Z) ──
    private readonly IOutboundPortUpdate _updateService = updateService ?? throw new ArgumentNullException(nameof(updateService));


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Checks asynchronously whether an application update is available and retrieves the latest version information.
    /// </summary>
    /// <returns>A <see cref="CheckUpdateResponse"/> indicating update availability and the latest version tag.</returns>
    public async Task<CheckUpdateResponse> CheckForUpdatesAsync()
    {
        var updateInfo = await _updateService.CheckForUpdateAsync().ConfigureAwait(false);
        return new CheckUpdateResponse(updateInfo.IsUpdateAvailable, updateInfo.LatestVersion);
    }

    /// <summary>
    /// Downloads and installs the latest application update asynchronously if an update is available.
    /// </summary>
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
