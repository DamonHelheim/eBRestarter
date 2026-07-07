using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Core.Application.Ports.Outbound.Browser;

namespace eBRestarter.Core.Application.Handlers;

public interface IRunBrowserPhaseHandler
{
    Task<BrowserPhaseResult> ExecuteAsync(ManageRestarterCycleRequest request, IBrowserOutboundPort? currentBrowser, Action<RestartTaskState, int, string?> reportProgress, CancellationToken token);
}
