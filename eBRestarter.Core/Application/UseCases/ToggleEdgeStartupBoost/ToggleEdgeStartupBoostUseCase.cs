using eBRestarter.Core.Application.Ports.Inbound.UseCases.ToggleEdgeStartupBoost;
using eBRestarter.Core.Application.Ports.Outbound.Browser;
using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models.Records;

namespace eBRestarter.Core.Application.UseCases.ToggleEdgeStartupBoost;

public sealed class ToggleEdgeStartupBoostUseCase(
    IBrowserConfigRepositoryOutboundPort startupService,
    IBrowserFactoryOutboundPort BrowserFactory) : IToggleEdgeStartupBoostUseCase
{
    private readonly IBrowserConfigRepositoryOutboundPort _startupService = startupService;
    private readonly IBrowserFactoryOutboundPort _browserFactory = BrowserFactory;

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




