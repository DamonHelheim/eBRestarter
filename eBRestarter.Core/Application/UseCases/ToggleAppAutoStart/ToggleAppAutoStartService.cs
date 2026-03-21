using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;

namespace eBRestarter.Core.Application.UseCases.ToggleAppAutoStart;

public class ToggleAppAutoStartService(
    IWindowsStartupManagerService startupManagerService,
    IEVisitorConfigService configService) : IToggleAppAutoStartUseCase
{
    private readonly IWindowsStartupManagerService _startupManagerService = startupManagerService;
    private readonly IEVisitorConfigService _configService = configService;

    public async Task<bool> InitializeAndGetStateAsync()
    {
        var config = _configService.LoadConfig();
        bool isEnabledInOs = await _startupManagerService.IsAutoStartEnabledAsync();

        if (config.Settings.StartWithWindows && !isEnabledInOs)
        {
            await _startupManagerService.EnableAutoStartAsync();
            return true;
        }

        return isEnabledInOs;
    }

    public async Task ToggleAsync(bool enable)
    {
        if (enable)
        {
            await _startupManagerService.EnableAutoStartAsync();
        }
        else
        {
            await _startupManagerService.DisableAutoStartAsync();
        }

        var config = _configService.LoadConfig();
        config.Settings.StartWithWindows = enable;
        _configService.SaveConfig(config);
    }
}