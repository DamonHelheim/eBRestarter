using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Ports.Outbound.Browser;
using eBRestarter.Core.Application.Ports.Inbound.Services;
using eBRestarter.Core.Application.Services;
using eBRestarter.Core.Application.Models.Errors;
using eBRestarter.Core.Application.Models.Records;

namespace eBRestarter.Core.Application.Ports.Inbound.Handlers;

public interface IRunBrowserPhaseHandler
{
    Task<BrowserPhaseResult> ExecuteAsync(ManageRestarterCycleRequest request, IBrowserOutboundPort? currentBrowser, Action<RestartTaskState, int, string?> reportProgress, CancellationToken token);
}

