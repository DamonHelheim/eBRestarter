using eBRestarter.Core.Application.Ports.Inbound.Interfaces.UseCases;
using eBRestarter.Core.Application.Ports.Outbound.Config;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;

namespace eBRestarter.Core.Application.UseCases;

public sealed class ToggleAppAutoStartUseCase(
    IOutboundPortAutoStartRepository startupManagerService,
    IOutboundPortEVisitorConfigRepository configService) : IUseCaseToggleAppAutoStart
{
    private readonly IOutboundPortAutoStartRepository _startupManagerService = startupManagerService;
    private readonly IOutboundPortEVisitorConfigRepository _configService = configService;

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


