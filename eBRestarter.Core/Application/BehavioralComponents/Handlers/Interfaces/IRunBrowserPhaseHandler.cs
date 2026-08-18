using System;
using System.Threading;
using System.Threading.Tasks;

using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;

namespace eBRestarter.Core.Application.BehavioralComponents.Handlers.Interfaces;

/// <summary>
/// Handler: Executes the browser running phase during a restart cycle.
/// </summary>
public interface IRunBrowserPhaseHandler
{
    /// <summary>
    /// Executes the browser running phase asynchronously, monitoring process liveness and cache cleanup triggers each second.
    /// </summary>
    /// <param name="request">The cycle request parameters defining runtime duration and options.</param>
    /// <param name="currentBrowser">The active browser port instance to monitor, if available.</param>
    /// <param name="reportProgress">Delegate callback action for reporting progress updates.</param>
    /// <param name="token">Cancellation token to observe for phase cancellation.</param>
    /// <returns>A <see cref="BrowserPhaseResult"/> indicating how the phase concluded.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> or <paramref name="reportProgress"/> is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException">Thrown when cancellation is requested via <paramref name="token"/>.</exception>
    Task<BrowserPhaseResult> ExecuteAsync(
        ManageRestarterCycleRequest request,
        IOutboundPortBrowser? currentBrowser,
        Action<RestartTaskState, int, string?> reportProgress,
        CancellationToken token);
}
