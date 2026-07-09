namespace eBRestarter.Core.Domain.Handlers;

/// <summary>
/// Calculation of the next restart schedule (pure logic, no I/O).
/// </summary>
public interface IRestartCalculationHandler
{
    /// <summary>
    /// Calculates the next restart date based on the interval (days) and time (hour).
    /// </summary>
    /// <param name="intervalDays">ComputerRestartIntervalDays (0 = disabled)</param>
    /// <param name="restartClockTime">RestartClockTime (hour 0-23)</param>
    /// <returns>The next date including time, or DateTime.MinValue if intervalDays &lt;= 0</returns>
    DateTime RetrieveNextRestartDate(int intervalDays, int restartClockTime);
}