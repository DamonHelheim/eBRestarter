namespace eBRestarter.Core.Application.Ports.Inbound.Handlers;

/// <summary>
/// Application facade for computing the next scheduled computer restart (delegates to domain calculation).
/// </summary>
public interface INextRestartDateHandler
{
    /// <summary>
    /// Computes the next restart instant from interval (days) and clock hour (0–23).
    /// </summary>
    /// <returns><see cref="DateTime.MinValue"/> when the schedule is disabled.</returns>
    DateTime RetrieveNextRestartDate(int intervalDays, int restartClockTime);
}

