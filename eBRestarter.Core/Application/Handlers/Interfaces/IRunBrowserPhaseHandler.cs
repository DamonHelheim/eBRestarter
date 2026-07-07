using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;

namespace eBRestarter.Core.Application.Handlers.Interfaces;

public interface IRunBrowserPhaseHandler
{
    Task<BrowserPhaseResult> ExecuteAsync(ManageRestarterCycleRequest request, IOutboundPortBrowser? currentBrowser, Action<RestartTaskState, int, string?> reportProgress, CancellationToken token);
}
