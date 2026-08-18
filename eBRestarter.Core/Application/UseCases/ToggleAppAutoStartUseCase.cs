using System;
using System.Threading.Tasks;

using eBRestarter.Core.Application.Ports.Inbound.Interfaces.UseCases;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;

namespace eBRestarter.Core.Application.UseCases;

/// <summary>
/// Use case implementation for querying and toggling the application's Windows autostart configuration state.
/// </summary>
/// <param name="configService">Outbound repository for accessing application configuration settings.</param>
/// <param name="startupManagerService">Outbound repository for operating system autostart configuration.</param>
public sealed class ToggleAppAutoStartUseCase(
    IOutboundPortEVisitorConfigRepository configService,
    IOutboundPortAutoStartRepository startupManagerService)
    : IUseCaseToggleAppAutoStart
{
    private readonly IOutboundPortEVisitorConfigRepository _configService = configService ?? throw new ArgumentNullException(nameof(configService));
    private readonly IOutboundPortAutoStartRepository _startupManagerService = startupManagerService ?? throw new ArgumentNullException(nameof(startupManagerService));

    /// <inheritdoc />
    public async Task<bool> InitializeAndGetStateAsync()
    {
        var config = _configService.LoadConfig();
        bool isEnabledInOs = await _startupManagerService.IsAutoStartEnabledAsync().ConfigureAwait(false);

        if (config?.Settings?.StartWithWindows == true && !isEnabledInOs)
        {
            await _startupManagerService.EnableAutoStartAsync().ConfigureAwait(false);
            return true;
        }

        return isEnabledInOs;
    }

    /// <inheritdoc />
    public async Task ToggleAsync(bool shouldEnable)
    {
        if (shouldEnable)
        {
            await _startupManagerService.EnableAutoStartAsync().ConfigureAwait(false);
        }
        else
        {
            await _startupManagerService.DisableAutoStartAsync().ConfigureAwait(false);
        }

        var config = _configService.LoadConfig();

        if (config?.Settings is not null)
        {
            config.Settings.StartWithWindows = shouldEnable;
            _configService.SaveConfig(config);
        }
    }
}
