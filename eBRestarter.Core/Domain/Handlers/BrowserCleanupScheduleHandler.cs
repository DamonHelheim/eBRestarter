namespace eBRestarter.Core.Domain.Handlers;

/// <summary>
/// Policy for the browser cache cleanup schedule.
/// </summary>
/// <param name="timeProvider">Time provider for evaluating local date and time.</param>
public sealed class BrowserCleanupScheduleHandler(TimeProvider timeProvider) : IBrowserCleanupScheduleHandler
{
    private readonly TimeProvider _timeProvider = timeProvider;

    /// <inheritdoc />
    public bool ShouldRunCleanupNow(int deleteBrowserCacheIntervalDays, DateTime nextBrowserDeleteCacheDate)
    {
        if (deleteBrowserCacheIntervalDays <= 0)
        {
            return false;
        }

        return _timeProvider.GetLocalNow().Date >= nextBrowserDeleteCacheDate.Date;
    }

    /// <inheritdoc />
    public DateTime CalculateNextCleanupDateAfterRun(DateTime fromDate, int intervalDays)
    {
        return fromDate.AddDays(intervalDays);
    }
}