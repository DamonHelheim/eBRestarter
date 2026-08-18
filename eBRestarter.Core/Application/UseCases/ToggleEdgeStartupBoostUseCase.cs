using System;

using eBRestarter.Core.Application.ObjectArchetypes.Constants;
using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.UseCases;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Logging;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;

namespace eBRestarter.Core.Application.UseCases;

/// <summary>
/// Use case implementation for querying Microsoft Edge installation state and toggling Edge Startup Boost.
/// </summary>
/// <param name="browserFactory">Factory for instantiating browser wrappers.</param>
/// <param name="logger">Application logger instance.</param>
/// <param name="startupService">Outbound repository for browser startup boost settings.</param>
public sealed class ToggleEdgeStartupBoostUseCase(
    IOutboundPortBrowserFactory browserFactory,
    IOutboundPortApplicationLogger<ToggleEdgeStartupBoostUseCase> logger,
    IOutboundPortBrowserConfigRepository startupService)
    : IUseCaseToggleEdgeStartupBoost
{
    private readonly IOutboundPortBrowserFactory _browserFactory = browserFactory ?? throw new ArgumentNullException(nameof(browserFactory));
    private readonly IOutboundPortApplicationLogger<ToggleEdgeStartupBoostUseCase> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOutboundPortBrowserConfigRepository _startupService = startupService ?? throw new ArgumentNullException(nameof(startupService));

    /// <inheritdoc />
    public bool IsEnabled() => _startupService.IsBrowserStartupBoostEnabled();

    /// <inheritdoc />
    public bool IsEdgeInstalled() => _browserFactory.Create(BrowserType.Edge).IsInstalled;

    /// <inheritdoc />
    public ToggleEdgeStartupBoostResponse Toggle(bool shouldEnable)
    {
        try
        {
            _startupService.SetBrowserStartupBoost(shouldEnable);
            return new ToggleEdgeStartupBoostResponse(true, shouldEnable);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                LogEventIds.OperatingSystem.EdgeStartupBoostChanged,
                ex,
                "Toggling Edge startup boost to {ShouldEnable} failed.",
                shouldEnable);

            return new ToggleEdgeStartupBoostResponse(false, !shouldEnable, ex.Message);
        }
    }
}
