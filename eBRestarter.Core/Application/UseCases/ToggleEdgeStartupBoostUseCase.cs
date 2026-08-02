using System;

using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.UseCases;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;

namespace eBRestarter.Core.Application.UseCases;

/// <summary>
/// Use case implementation for querying Microsoft Edge installation state and toggling Edge Startup Boost.
/// </summary>
public sealed class ToggleEdgeStartupBoostUseCase(
    IOutboundPortBrowserFactory browserFactory,
    IOutboundPortBrowserConfigRepository startupService) : IUseCaseToggleEdgeStartupBoost
{
    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    // ── Block 1: Injizierte Abhängigkeiten (alphabetisch A–Z) ──
    private readonly IOutboundPortBrowserFactory _browserFactory = browserFactory ?? throw new ArgumentNullException(nameof(browserFactory));
    private readonly IOutboundPortBrowserConfigRepository _startupService = startupService ?? throw new ArgumentNullException(nameof(startupService));


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Checks whether Microsoft Edge Startup Boost is currently enabled in system settings.
    /// </summary>
    /// <returns><c>true</c> if startup boost is enabled; otherwise, <c>false</c>.</returns>
    public bool IsEnabled() => _startupService.IsBrowserStartupBoostEnabled();

    /// <summary>
    /// Checks whether Microsoft Edge is installed on the current host system.
    /// </summary>
    /// <returns><c>true</c> if Edge is installed; otherwise, <c>false</c>.</returns>
    public bool IsEdgeInstalled() => _browserFactory.Create(BrowserType.Edge).IsInstalled;

    /// <summary>
    /// Toggles Microsoft Edge Startup Boost setting on or off.
    /// </summary>
    /// <param name="shouldEnable"><c>true</c> to enable startup boost; <c>false</c> to disable startup boost.</param>
    /// <returns>A <see cref="ToggleEdgeStartupBoostResponse"/> indicating success status and effective state.</returns>
    public ToggleEdgeStartupBoostResponse Toggle(bool shouldEnable)
    {
        try
        {
            _startupService.SetBrowserStartupBoost(shouldEnable);
            return new ToggleEdgeStartupBoostResponse(true, shouldEnable);
        }
        catch (Exception ex)
        {
            return new ToggleEdgeStartupBoostResponse(false, !shouldEnable, ex.Message);
        }
    }
}
