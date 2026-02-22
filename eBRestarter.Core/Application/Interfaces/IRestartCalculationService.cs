namespace eBRestarter.Core.Application.Interfaces;

/// <summary>
/// Port für die Berechnung des nächsten Neustart-Termins (reine Berechnungslogik, kein I/O).
/// </summary>
public interface IRestartCalculationService
{
    // =========================================================
    // 1. PUBLIC METHODS (API / Vertrag)
    // =========================================================
    #region PublicMethods

    /// <summary>
    /// Berechnet das nächste Neustart-Datum basierend auf Intervall (Tage) und Uhrzeit (Stunde).
    /// </summary>
    /// <param name="intervalDays">ComputerRestartIntervalDays (0 = deaktiviert)</param>
    /// <param name="restartClockTime">RestartClockTime (Stunde 0-23)</param>
    /// <returns>Nächstes Datum inkl. Uhrzeit, oder DateTime.MinValue wenn intervalDays &lt;= 0</returns>
    DateTime GetNextRestartDate(int intervalDays, int restartClockTime);

    #endregion
}
