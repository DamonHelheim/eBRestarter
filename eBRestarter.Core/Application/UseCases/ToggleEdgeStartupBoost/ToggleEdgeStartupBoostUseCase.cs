using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Application.Enums;

namespace eBRestarter.Core.Application.UseCases.ToggleEdgeStartupBoost;

public class ToggleEdgeStartupBoostUseCase(
    IWindowsStartupManagerService startupService,
    IBrowserFactory browserFactory) : IToggleEdgeStartupBoostUseCase
{
    private readonly IWindowsStartupManagerService _startupService = startupService;
    private readonly IBrowserFactory _browserFactory = browserFactory;

    public bool IsEnabled() =>
        _startupService.IsEdgeStartupBoostEnabled();

    public bool IsEdgeInstalled()
    {
        var edge = _browserFactory.Create(BrowserType.Edge);

        return edge.IsInstalled;
    }

    public ToggleEdgeStartupBoostResponse Toggle(bool enable)
    {
        try
        {
            _startupService.SetEdgeStartupBoost(enable);

            return new ToggleEdgeStartupBoostResponse(true, enable);

        }
        catch (Exception ex)
        {
            return new ToggleEdgeStartupBoostResponse(false, !enable, ex.Message);
        }
    }
}
