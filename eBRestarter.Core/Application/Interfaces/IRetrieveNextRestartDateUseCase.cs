namespace eBRestarter.Core.Application.Interfaces;

/// <summary>
/// Application facade for computing the next scheduled computer restart (delegates to domain calculation).
/// </summary>
public interface IRetrieveNextRestartDateUseCase
{
    /// <summary>
    /// Computes the next restart instant from interval (days) and clock hour (0â€“23).
    /// </summary>
    /// <returns><see cref="DateTime.MinValue"/> when the schedule is disabled.</returns>
    DateTime RetrieveNextRestartDate(int intervalDays, int restartClockTime);
}
