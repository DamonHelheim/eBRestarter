using System;
using System.Threading.Tasks;

using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;

namespace eBRestarter.Core.Application.BehavioralComponents.Providers;

/// <summary>
/// Aggregates system hardware, operating system, and runtime info asynchronously for display in the application UI.
/// </summary>
/// <param name="hardwareInfoProvider">The provider for system hardware details (CPU, GPU, RAM).</param>
/// <param name="osEditionProvider">The provider for operating system edition details.</param>
/// <param name="systemInfoProvider">The provider for OS version and default browser details.</param>
public sealed class SystemInformationProvider(
    IOutboundPortHardwareInfoProvider hardwareInfoProvider,
    IOutboundPortOsEditionProvider osEditionProvider,
    IOutboundPortSystemInfoProvider systemInfoProvider) : IInboundPortSystemInformationProvider
{
    private readonly IOutboundPortHardwareInfoProvider _hardwareInfoProvider = hardwareInfoProvider ?? throw new ArgumentNullException(nameof(hardwareInfoProvider));
    private readonly IOutboundPortOsEditionProvider _osEditionProvider = osEditionProvider ?? throw new ArgumentNullException(nameof(osEditionProvider));
    private readonly IOutboundPortSystemInfoProvider _systemInfoProvider = systemInfoProvider ?? throw new ArgumentNullException(nameof(systemInfoProvider));

    /// <inheritdoc />
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