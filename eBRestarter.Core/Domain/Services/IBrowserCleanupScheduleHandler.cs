namespace eBRestarter.Core.Domain.Services;

/// <summary>
/// Logik „Wann Browser-Cache-Löschung ausführen?" und „Nächstes Löschdatum berechnen" (reine Policy).
/// </summary>
public interface IBrowserCleanupScheduleHandler
{
    /// <summary>
    /// Prüft, ob das Feature aktiv ist (Interval &gt; 0) und das nächste Löschdatum erreicht oder überschritten ist.
    /// </summary>
    bool ShouldRunCleanupNow(int deleteBrowserCacheIntervalDays, DateTime nextBrowserDeleteCacheDate);

    /// <summary>
    /// Berechnet das nächste Löschdatum nach einem Durchlauf (fromDate + intervalDays).
    /// </summary>
    DateTime CalculateNextCleanupDateAfterRun(DateTime fromDate, int intervalDays);
}
