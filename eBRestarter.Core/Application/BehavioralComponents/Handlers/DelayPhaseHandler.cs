using System;
using System.Threading;
using System.Threading.Tasks;

using eBRestarter.Core.Application.BehavioralComponents.Handlers.Interfaces;
using eBRestarter.Core.Application.ObjectArchetypes.Enums;

namespace eBRestarter.Core.Application.BehavioralComponents.Handlers;

/// <summary>
/// Handles the delay phase execution of a restart task with progress reporting and cancellation support.
/// </summary>
public sealed class DelayPhaseHandler(TimeProvider timeProvider) : IDelayPhaseHandler
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    private static readonly TimeSpan OneSecondInterval = TimeSpan.FromSeconds(1);

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    private readonly TimeProvider _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Executes the delay phase asynchronously for the specified duration in seconds.
    /// </summary>
    /// <param name="state">The current state of the restart task.</param>
    /// <param name="totalSeconds">The total duration of the delay phase in seconds.</param>
    /// <param name="reportProgress">Delegate action to report progress updates back to the caller.</param>
    /// <param name="token">Cancellation token to observe while waiting for the delay to complete.</param>
    public async Task ExecuteAsync(
        RestartTaskState state,
        int totalSeconds,
        Action<RestartTaskState, int, string?> reportProgress,
        CancellationToken token)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(totalSeconds);
        ArgumentNullException.ThrowIfNull(reportProgress);

        if (totalSeconds == 0)
        {
            token.ThrowIfCancellationRequested();
            reportProgress(state, 0, null);
            return;
        }

        using var timer = new PeriodicTimer(OneSecondInterval, _timeProvider);

        for (int secondsRemaining = totalSeconds; secondsRemaining >= 0; secondsRemaining--)
        {
            token.ThrowIfCancellationRequested();
            reportProgress(state, secondsRemaining, null);

            if (secondsRemaining > 0)
            {
                await timer.WaitForNextTickAsync(token);
            }
        }
    }
}
