using System;
using System.Threading;
using System.Threading.Tasks;

using eBRestarter.Core.Application.ObjectArchetypes.Enums;

namespace eBRestarter.Core.Application.BehavioralComponents.Handlers.Interfaces;

/// <summary>
/// Defines the contract for executing the delay phase of a restart task with progress reporting and cancellation support.
/// </summary>
public interface IDelayPhaseHandler
{
    /// <summary>
    /// Executes the delay phase asynchronously for the specified duration in seconds.
    /// </summary>
    /// <param name="state">The current state of the restart task.</param>
    /// <param name="totalSeconds">The total duration of the delay phase in seconds.</param>
    /// <param name="reportProgress">Delegate action to report progress updates back to the caller.</param>
    /// <param name="token">Cancellation token to observe while waiting for the delay to complete.</param>
    /// <returns>A task that represents the asynchronous delay execution.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="totalSeconds"/> is negative.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="reportProgress"/> is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException">Thrown when cancellation is requested via <paramref name="token"/>.</exception>
    Task ExecuteAsync(RestartTaskState state, int totalSeconds, Action<RestartTaskState, int, string?> reportProgress, CancellationToken token);
}
