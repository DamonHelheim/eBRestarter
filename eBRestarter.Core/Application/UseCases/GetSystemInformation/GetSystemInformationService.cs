using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;

namespace eBRestarter.Core.Application.UseCases.GetSystemInformation;

public class GetSystemInformationService(
    IHardwareInfoService hardwareService,
    IOsEditionService osEditionService,
    IWindowsSystemInfoService systemInfoService) : IGetSystemInformationUseCase
{
    private readonly IHardwareInfoService _hardwareService = hardwareService;
    private readonly IOsEditionService _osEditionService = osEditionService;
    private readonly IWindowsSystemInfoService _systemInfoService = systemInfoService;

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
