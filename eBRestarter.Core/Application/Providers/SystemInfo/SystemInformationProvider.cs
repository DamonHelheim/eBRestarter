using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using eBRestarter.Core.Application.Ports.Outbound.SystemInfo;

namespace eBRestarter.Core.Application.Providers.SystemInfo;

using eBRestarter.Core.Application.Models.Records;

public class SystemInformationProvider(
    IHardwareInfoPort hardwareService,
    IOsEditionPort osEditionService,
    ISystemInfoPort systemInfoService) : ISystemInformationPort
{
    private readonly IHardwareInfoPort _hardwareService = hardwareService;
    private readonly IOsEditionPort _osEditionService = osEditionService;
    private readonly ISystemInfoPort _systemInfoService = systemInfoService;

    public async Task<SystemInformationResponse> ExecuteAsync()
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








