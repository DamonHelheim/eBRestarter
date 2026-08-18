namespace eBRestarter.Core.Domain.Handlers;

/// <summary>
/// Pure calculation logic for the next restart schedule.
/// </summary>
/// <param name="timeProvider">Time provider for evaluating local date and time.</param>
public sealed class RestartCalculationHandler(TimeProvider timeProvider) : IRestartCalculationHandler
{
    private readonly TimeProvider _timeProvider = timeProvider;

    /// <inheritdoc />
    public DateTime RetrieveNextRestartDate(int intervalDays, int restartClockTime)
    {
        if (intervalDays > 0)
        {
            return _timeProvider.GetLocalNow().Date.AddDays(intervalDays).AddHours(restartClockTime);
        }

        return DateTime.MinValue;
    }
}