using eBRestarter.Core.Application.Ports.Outbound.Providers;
using eBRestarter.Core.Application.Ports.Inbound.UseCases.ToggleAppAutoStart;
using eBRestarter.Core.Application.Ports.Outbound.Config;
using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;

namespace eBRestarter.Core.Application.UseCases.ToggleAppAutoStart;

using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models.Errors;

public sealed class ToggleAppAutoStartUseCase(
    IAutoStartPort startupManagerService,
    IEVisitorConfigPort configService) : IToggleAppAutoStartUseCase
{
    private readonly IAutoStartPort _startupManagerService = startupManagerService;
    private readonly IEVisitorConfigPort _configService = configService;

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


