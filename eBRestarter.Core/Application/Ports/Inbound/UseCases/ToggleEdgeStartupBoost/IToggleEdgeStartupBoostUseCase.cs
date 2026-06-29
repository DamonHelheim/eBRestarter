using eBRestarter.Core.Application.Models.Records;

namespace eBRestarter.Core.Application.Ports.Inbound.UseCases.ToggleEdgeStartupBoost;

public interface IToggleEdgeStartupBoostUseCase
{
    bool IsEnabled();
    bool IsEdgeInstalled();
    ToggleEdgeStartupBoostResponse Toggle(bool enable);
}

