namespace eBRestarter.Core.Domain.Handlers;

/// <summary>
/// Logic for "When should the browser cache cleanup be executed?" and "Calculate next cleanup date" (pure Policy).
/// </summary>
public interface IBrowserCleanupScheduleHandler
{
    /// <summary>
    /// Checks whether the feature is active (Interval &gt; 0) and if the next cleanup date has been reached or passed.
    /// </summary>
    /// <param name="deleteBrowserCacheIntervalDays">The configured cleanup interval in days (0 = disabled).</param>
    /// <param name="nextBrowserDeleteCacheDate">The next scheduled cleanup date.</param>
    /// <returns><c>true</c> if cleanup should execute immediately; otherwise, <c>false</c>.</returns>
    bool ShouldRunCleanupNow(int deleteBrowserCacheIntervalDays, DateTime nextBrowserDeleteCacheDate);

    /// <summary>
    /// Calculates the next cleanup date after a run (fromDate + intervalDays).
    /// </summary>
    /// <param name="fromDate">The base date from which to calculate the next cleanup date.</param>
    /// <param name="intervalDays">The cleanup interval in days.</param>
    /// <returns>The calculated next <see cref="DateTime"/> for browser cache cleanup.</returns>
    DateTime CalculateNextCleanupDateAfterRun(DateTime fromDate, int intervalDays);
}