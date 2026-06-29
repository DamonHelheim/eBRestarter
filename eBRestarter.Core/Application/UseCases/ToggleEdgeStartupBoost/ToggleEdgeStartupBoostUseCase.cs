using eBRestarter.Core.Application.Ports.Outbound.Providers;
using eBRestarter.Core.Application.Ports.Inbound.UseCases.ToggleEdgeStartupBoost;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using eBRestarter.Core.Application.Ports.Outbound.Browser;

namespace eBRestarter.Core.Application.UseCases.ToggleEdgeStartupBoost;

using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models.Errors;

public sealed class ToggleEdgeStartupBoostUseCase(
    IBrowserConfigPort startupService,
    IBrowserFactoryPort BrowserFactory) : IToggleEdgeStartupBoostUseCase
{
    private readonly IBrowserConfigPort _startupService = startupService;
    private readonly IBrowserFactoryPort _browserFactory = BrowserFactory;

    public bool IsEnabled() =>
        _startupService.IsBrowserStartupBoostEnabled();

    public bool IsEdgeInstalled()
    {
        var edge = _browserFactory.Create(BrowserType.Edge);

        return edge.IsInstalled;
    }

    public ToggleEdgeStartupBoostResponse Toggle(bool enable)
    {
        try
        {
            _startupService.SetBrowserStartupBoost(enable);

            return new ToggleEdgeStartupBoostResponse(true, enable);

        }
        catch (Exception ex)
        {
            return new ToggleEdgeStartupBoostResponse(false, !enable, ex.Message);
        }
    }
}




