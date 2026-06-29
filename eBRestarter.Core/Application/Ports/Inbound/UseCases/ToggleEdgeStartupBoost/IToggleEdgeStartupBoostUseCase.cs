namespace eBRestarter.Core.Application.Ports.Inbound.UseCases.ToggleEdgeStartupBoost;

using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models.Errors;

public interface IToggleEdgeStartupBoostUseCase
{
    bool IsEnabled();
    bool IsEdgeInstalled();
    ToggleEdgeStartupBoostResponse Toggle(bool enable);
}

