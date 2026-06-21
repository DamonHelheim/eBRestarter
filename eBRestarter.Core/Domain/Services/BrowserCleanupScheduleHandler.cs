namespace eBRestarter.Core.Domain.Services;

/// <summary>
/// Policy für Browser-Cache-Lösch-Zeitplan.
/// </summary>
public class BrowserCleanupScheduleHandler(TimeProvider timeProvider) : IBrowserCleanupScheduleHandler
{
    private readonly TimeProvider _timeProvider = timeProvider;

    /// <inheritdoc />
    public bool ShouldRunCleanupNow(int deleteBrowserCacheIntervalDays, DateTime nextBrowserDeleteCacheDate)
    {
        if (deleteBrowserCacheIntervalDays <= 0) return false;

        return _timeProvider.GetLocalNow().Date >= nextBrowserDeleteCacheDate.Date;
    }

    /// <inheritdoc />
    public DateTime CalculateNextCleanupDateAfterRun(DateTime fromDate, int intervalDays)
    {
        return fromDate.AddDays(intervalDays);
    }
}
