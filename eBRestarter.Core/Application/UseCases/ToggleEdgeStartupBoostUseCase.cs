using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.UseCases;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;

namespace eBRestarter.Core.Application.UseCases;

public sealed class ToggleEdgeStartupBoostUseCase(
    IOutboundPortBrowserConfigRepository startupService,
    IOutboundPortBrowserFactory BrowserFactory) : IUseCaseToggleEdgeStartupBoost
{
    private readonly IOutboundPortBrowserConfigRepository _startupService = startupService;
    private readonly IOutboundPortBrowserFactory _browserFactory = BrowserFactory;

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




