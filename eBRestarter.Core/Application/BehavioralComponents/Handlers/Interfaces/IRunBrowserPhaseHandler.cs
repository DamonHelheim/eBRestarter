using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;

namespace eBRestarter.Core.Application.BehavioralComponents.Handlers.Interfaces;

public interface IRunBrowserPhaseHandler
{
    Task<BrowserPhaseResult> ExecuteAsync(ManageRestarterCycleRequest request, IOutboundPortBrowser? currentBrowser, Action<RestartTaskState, int, string?> reportProgress, CancellationToken token);
}
