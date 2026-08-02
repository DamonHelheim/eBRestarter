using System;
using System.Threading.Tasks;

using eBRestarter.Core.Application.Ports.Inbound.Interfaces.UseCases;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;

namespace eBRestarter.Core.Application.UseCases;

/// <summary>
/// Use case implementation for querying and toggling the application's Windows autostart configuration state.
/// </summary>
public sealed class ToggleAppAutoStartUseCase(
    IOutboundPortEVisitorConfigRepository configService,
    IOutboundPortAutoStartRepository startupManagerService)
    : IUseCaseToggleAppAutoStart
{
    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════

    // ── Block 1: Injizierte Abhängigkeiten (alphabetisch A–Z) ──
    private readonly IOutboundPortEVisitorConfigRepository _configService = configService ?? throw new ArgumentNullException(nameof(configService));
    private readonly IOutboundPortAutoStartRepository _startupManagerService = startupManagerService ?? throw new ArgumentNullException(nameof(startupManagerService));


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Initializes and retrieves the effective autostart state asynchronously, enabling autostart if configured in settings but disabled in OS.
    /// </summary>
    /// <returns><c>true</c> if autostart is enabled in the OS after initialization; otherwise, <c>false</c>.</returns>
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

    /// <summary>
    /// Toggles the Windows autostart state asynchronously in both OS settings and internal application configuration.
    /// </summary>
    /// <param name="shouldEnable"><c>true</c> to enable autostart; <c>false</c> to disable autostart.</param>
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
