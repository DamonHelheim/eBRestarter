namespace eBRestarter.Core.Domain.Services;

/// <summary>
/// Reine Berechnungslogik für den nächsten Neustart-Termin.
/// </summary>
public class RestartCalculationService(TimeProvider timeProvider) : IRestartCalculationService
{
    private readonly TimeProvider _timeProvider = timeProvider;

    /// <inheritdoc />
    public DateTime GetNextRestartDate(int intervalDays, int restartClockTime)
    {
        if (intervalDays > 0)
        {
            return _timeProvider.GetLocalNow().Date.AddDays(intervalDays).AddHours(restartClockTime);
        }

        return DateTime.MinValue;
    }
}
