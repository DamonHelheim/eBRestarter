namespace eBRestarter.Core.Application.UseCases.ToggleEdgeStartupBoost;

public interface IToggleEdgeStartupBoostUseCase
{
    bool IsEnabled();
    bool IsEdgeInstalled();
    ToggleEdgeStartupBoostResponse Toggle(bool enable);
}
