using eBRestarter.Core.Application.BehavioralComponents.Handlers.Interfaces;
using eBRestarter.Core.Application.ObjectArchetypes.Enums;

namespace eBRestarter.Core.Application.BehavioralComponents.Handlers;

public sealed class DelayPhaseHandler(TimeProvider timeProvider) : IDelayPhaseHandler
{
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task ExecuteAsync(RestartTaskState state, int totalSeconds, Action<RestartTaskState, int, string?> reportProgress, CancellationToken token)
    {
        for (int secondsRemaining = totalSeconds; secondsRemaining >= 0; secondsRemaining--)
        {
            token.ThrowIfCancellationRequested();
            reportProgress(state, secondsRemaining, null);

            if (secondsRemaining > 0)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1000), _timeProvider, token);
            }
        }
    }
}
