using System;
using System.Threading.Tasks;

using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;

namespace eBRestarter.Core.Application.BehavioralComponents.Providers;

/// <summary>
/// Aggregates system hardware, operating system, and runtime info asynchronously for display in the application UI.
/// </summary>
public sealed class SystemInformationProvider(
    IOutboundPortHardwareInfoProvider hardwareInfoProvider,
    IOutboundPortOsEditionProvider osEditionProvider,
    IOutboundPortSystemInfoProvider systemInfoProvider) : IInboundPortSystemInformationProvider
{
    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    private readonly IOutboundPortHardwareInfoProvider _hardwareInfoProvider = hardwareInfoProvider ?? throw new ArgumentNullException(nameof(hardwareInfoProvider));
    private readonly IOutboundPortOsEditionProvider _osEditionProvider = osEditionProvider ?? throw new ArgumentNullException(nameof(osEditionProvider));
    private readonly IOutboundPortSystemInfoProvider _systemInfoProvider = systemInfoProvider ?? throw new ArgumentNullException(nameof(systemInfoProvider));


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Retrieves aggregated hardware, OS edition, OS version, and browser information asynchronously.
    /// </summary>
    /// <returns>A <see cref="SystemInformationResponse"/> containing the aggregated system details.</returns>
    public async Task<SystemInformationResponse> RetrieveAsync()
    {
        var hardwareTask = _hardwareInfoProvider.RetrieveHardwareInfoAsync();
        var editionTask = _osEditionProvider.RetrieveOsEditionAsync();

        await Task.WhenAll(hardwareTask, editionTask);

        var hardware = await hardwareTask;
        var edition = await editionTask;

        var osDisplayVersion = _systemInfoProvider.RetrieveCurrentOsDisplayVersion();
        var osBuildVersion = _systemInfoProvider.RetrieveCurrentOsBuildVersion();
        var browserName = _systemInfoProvider.RetrieveCurrentStandardBrowserName();

        return new SystemInformationResponse(
            ProcessorName: hardware.ProcessorName,
            GraphicsCardName: hardware.GraphicsCardName,
            InstalledRam: hardware.InstalledRam,
            OsEdition: edition,
            OsDisplayVersion: osDisplayVersion,
            OsBuildVersion: osBuildVersion,
            StandardBrowserName: browserName);
    }
}