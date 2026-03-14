using System;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Domain.Models.Records.Config;

namespace eBRestarter.Core.Application.Services;

/// <summary>
/// Logik für Browser-Cache-Lösch-Zeitplan (Move aus ViewModelRestartTask).
/// </summary>
public class BrowserCleanupScheduleService : IBrowserCleanupScheduleService
{
    private readonly TimeProvider _timeProvider;

    public BrowserCleanupScheduleService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    // =========================================================
    // 1. PUBLIC & PROTECTED METHODS (API)
    // =========================================================
    #region PublicAndProtectedMethods

    /// <inheritdoc />
    public bool ShouldRunCleanupNow(AppConfig config)
    {
        if (config?.Browser == null || config.Browser.DeleteBrowserCacheIntervalDays <= 0)
            return false;

        return _timeProvider.GetLocalNow().Date >= config.Browser.NextBrowserDeleteCacheDate;
    }

    /// <inheritdoc />
    public DateTime GetNextCleanupDateAfterRun(DateTime fromDate, int intervalDays)
    {
        return fromDate.AddDays(intervalDays);
    }

    #endregion
}
