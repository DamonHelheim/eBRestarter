namespace eBRestarter.Core.Domain.Handlers;

/// <summary>
/// Logic for "When should the browser cache cleanup be executed?" and "Calculate next cleanup date" (pure Policy).
/// </summary>
public interface IBrowserCleanupScheduleHandler
{
    /// <summary>
    /// Checks whether the feature is active (Interval &gt; 0) and if the next cleanup date has been reached or passed.
    /// </summary>
    bool ShouldRunCleanupNow(int deleteBrowserCacheIntervalDays, DateTime nextBrowserDeleteCacheDate);

    /// <summary>
    /// Calculates the next cleanup date after a run (fromDate + intervalDays).
    /// </summary>
    DateTime CalculateNextCleanupDateAfterRun(DateTime fromDate, int intervalDays);
}