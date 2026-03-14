using System.Threading.Tasks;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;

namespace eBRestarter.Core.Application.UseCases.GetSystemInformation;

public class GetSystemInformationService : IGetSystemInformationUseCase
{
    private readonly IHardwareInfoService _hardwareService;
    private readonly IOsEditionService _osEditionService;
    private readonly IWindowsSystemInfoService _systemInfoService;

    public GetSystemInformationService(
        IHardwareInfoService hardwareService,
        IOsEditionService osEditionService,
        IWindowsSystemInfoService systemInfoService)
    {
        _hardwareService = hardwareService;
        _osEditionService = osEditionService;
        _systemInfoService = systemInfoService;
    }

    public async Task<SystemInformationResponse> ExecuteAsync()
    {
        var hardware = await _hardwareService.GetHardwareInfoAsync();
        var edition = await _osEditionService.GetOsEditionAsync();
        var osDisplayVersion = _systemInfoService.GetCurrentOsDisplayVersion();
        var osBuildVersion = _systemInfoService.GetCurrentOsBuildVersion();
        var browserName = _systemInfoService.GetCurrentStandardBrowserName();

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
