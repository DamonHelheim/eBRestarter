using eBRestarter.Core.Domain.Models.Records.Config;
using System;

namespace eBRestarter.Core.Application.Interfaces
{
    /// <summary>
    /// Port für die Logik "Wann Browser-Cache-Löschung ausführen?" und "Nächstes Löschdatum berechnen".
    /// </summary>
    public interface IBrowserCleanupScheduleService
    {
        // =========================================================
        // 1. PUBLIC METHODS (API / Vertrag)
        // =========================================================
        #region PublicMethods

        /// <summary>
        /// Prüft, ob das Feature aktiv ist (Interval &gt; 0) und das nächste Löschdatum erreicht oder überschritten ist.
        /// </summary>
        bool ShouldRunCleanupNow(AppConfig config);

        /// <summary>
        /// Berechnet das nächste Löschdatum nach einem Durchlauf (fromDate + intervalDays).
        /// </summary>
        DateTime GetNextCleanupDateAfterRun(DateTime fromDate, int intervalDays);

        #endregion
    }
}
