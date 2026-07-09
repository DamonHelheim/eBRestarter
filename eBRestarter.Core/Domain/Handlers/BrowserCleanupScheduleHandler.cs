namespace eBRestarter.Core.Domain.Handlers;

/// <summary>
/// Policy for the browser cache cleanup schedule.
/// </summary>
public sealed class BrowserCleanupScheduleHandler(TimeProvider timeProvider) : IBrowserCleanupScheduleHandler
{
    private readonly TimeProvider _timeProvider = timeProvider;

    public bool ShouldRunCleanupNow(int deleteBrowserCacheIntervalDays, DateTime nextBrowserDeleteCacheDate)
    {
        if (deleteBrowserCacheIntervalDays <= 0) return false;

        return _timeProvider.GetLocalNow().Date >= nextBrowserDeleteCacheDate.Date;
    }

    public DateTime CalculateNextCleanupDateAfterRun(DateTime fromDate, int intervalDays)
    {
        return fromDate.AddDays(intervalDays);
    }
}