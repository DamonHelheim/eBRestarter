using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using eBRestarter.Core.Application.Ports.Inbound.Providers;
using eBRestarter.Core.Application.Models.Records;

namespace eBRestarter.Core.Application.Providers;

public class SystemInformationProvider(
    IHardwareInfoProviderOutboundPort hardwareInfoProvider,
    IOsEditionProviderOutboundPort osEditionProvider,
    ISystemInfoProviderOutboundPort systemInfoProvider) : IInboundPortSystemInformationProvider
{
    private readonly IHardwareInfoProviderOutboundPort _hardwareService = hardwareInfoProvider;
    private readonly IOsEditionProviderOutboundPort _osEditionService = osEditionProvider;
    private readonly ISystemInfoProviderOutboundPort _systemInfoService = systemInfoProvider;

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