using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;

namespace eBRestarter.Core.Application.Providers;

public class SystemInformationProvider(
    IOutboundPortHardwareInfoProvider hardwareInfoProvider,
    IOutboundPortOsEditionProvider osEditionProvider,
    IOutboundPortSystemInfoProvider systemInfoProvider) : IInboundPortSystemInformationProvider
{
    private readonly IOutboundPortHardwareInfoProvider _hardwareService = hardwareInfoProvider;
    private readonly IOutboundPortOsEditionProvider _osEditionService = osEditionProvider;
    private readonly IOutboundPortSystemInfoProvider _systemInfoService = systemInfoProvider;

    public async Task<SystemInformationResponse> RetrieveAsync()
    {
        var hardware = await _hardwareService.RetrieveHardwareInfoAsync();
        var edition = await _osEditionService.RetrieveOsEditionAsync();
        var osDisplayVersion = _systemInfoService.RetrieveCurrentOsDisplayVersion();
        var osBuildVersion = _systemInfoService.RetrieveCurrentOsBuildVersion();
        var browserName = _systemInfoService.RetrieveCurrentStandardBrowserName();

        return new SystemInformationResponse(
            ProcessorName: hardware.ProcessorName,
            GraphicsCardName: hardware.GraphicsCardName,
            InstalledRam: hardware.InstalledRam,
            OsEdition: edition,
            OsDisplayVersion: osDisplayVersion,
            OsBuildVersion: osBuildVersion,
            StandardBrowserName: browserName
        );
    }
}