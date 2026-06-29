using eBRestarter.Core.Application.Enums;

namespace eBRestarter.Core.Application.Handlers.ManageRestarterCycle.Strategies;

public sealed class DelayPhaseStrategy(TimeProvider timeProvider) : IDelayPhaseStrategy
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
