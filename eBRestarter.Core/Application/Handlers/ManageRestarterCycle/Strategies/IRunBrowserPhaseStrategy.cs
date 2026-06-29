using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Ports.Outbound.Browser;
using eBRestarter.Core.Application.Ports.Inbound.UseCases.ManageRestarterCycle;
using eBRestarter.Core.Application.UseCases.ManageRestarterCycle;
using eBRestarter.Core.Application.Models.Errors;
using eBRestarter.Core.Application.Models.Records;

namespace eBRestarter.Core.Application.Handlers.ManageRestarterCycle.Strategies;

public interface IRunBrowserPhaseStrategy
{
    Task<BrowserPhaseResult> ExecuteAsync(ManageRestarterCycleRequest request, IBrowserPort? currentBrowser, Action<RestartTaskState, int, string?> reportProgress, CancellationToken token);
}

