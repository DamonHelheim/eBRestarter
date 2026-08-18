using System;
using System.Threading;
using System.Threading.Tasks;

using eBRestarter.Core.Application.BehavioralComponents.Handlers.Interfaces;
using eBRestarter.Core.Application.ObjectArchetypes.Enums;

namespace eBRestarter.Core.Application.BehavioralComponents.Handlers;

/// <summary>
/// Handles the delay phase execution of a restart task with progress reporting and cancellation support.
/// </summary>
/// <param name="timeProvider">The time provider used for time abstraction and periodic timer generation.</param>
public sealed class DelayPhaseHandler(TimeProvider timeProvider) : IDelayPhaseHandler
{
    private static readonly TimeSpan OneSecondInterval = TimeSpan.FromSeconds(1);
    private readonly TimeProvider _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));

    /// <inheritdoc />
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
