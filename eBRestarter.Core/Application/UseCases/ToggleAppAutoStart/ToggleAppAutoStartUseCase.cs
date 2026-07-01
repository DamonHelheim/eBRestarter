using eBRestarter.Core.Application.Ports.Inbound.UseCases.ToggleAppAutoStart;
using eBRestarter.Core.Application.Ports.Outbound.Config;
using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;

namespace eBRestarter.Core.Application.UseCases.ToggleAppAutoStart;

public sealed class ToggleAppAutoStartUseCase(
    IAutoStartRepositoryOutboundPort startupManagerService,
    IEVisitorConfigRepositoryOutboundPort configService) : IToggleAppAutoStartUseCase
{
    private readonly IAutoStartRepositoryOutboundPort _startupManagerService = startupManagerService;
    private readonly IEVisitorConfigRepositoryOutboundPort _configService = configService;

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


