namespace eBRestarter.Core.Domain.Services;

/// <summary>
/// Berechnung des nächsten Neustart-Termins (reine Logik, kein I/O).
/// </summary>
public interface IRestartCalculationService
{
    /// <summary>
    /// Berechnet das nächste Neustart-Datum basierend auf Intervall (Tage) und Uhrzeit (Stunde).
    /// </summary>
    /// <param name="intervalDays">ComputerRestartIntervalDays (0 = deaktiviert)</param>
    /// <param name="restartClockTime">RestartClockTime (Stunde 0-23)</param>
    /// <returns>Nächstes Datum inkl. Uhrzeit, oder DateTime.MinValue wenn intervalDays &lt;= 0</returns>
    DateTime GetNextRestartDate(int intervalDays, int restartClockTime);
}
