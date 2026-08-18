namespace eBRestarter.Core.Application.Ports.Inbound.Interfaces.Handlers;

/// <summary>
/// Application facade for computing the next scheduled computer restart (delegates to domain calculation).
/// </summary>
/// <remarks>
/// <b>Architectural Classification: INBOUND PORT / USE CASE INTERFACE</b>
/// <list type="bullet">
/// <item><b>Consumer:</b> Presentation layer (<see cref="eBRestarter.Desktop.WinUI3.ViewModels.ViewModelOptionsGeneral"/> via MVVM).</item>
/// <item><b>Implementer:</b> Application Core (<see cref="BehavioralComponents.Handlers.NextRestartDateHandler"/>).</item>
/// <item><b>Rationale:</b> Serves as an entry point into the application core to compute and display the next scheduled restart time for the UI.</item>
/// </list>
/// </remarks>
public interface IInboundPortNextRestartDateHandler
{
    /// <summary>
    /// Computes the next restart instant from interval (days) and clock hour (0–23).
    /// </summary>
    /// <param name="intervalDays">The interval in days between restarts.</param>
    /// <param name="restartClockTime">The target clock time hour (0–23) for the restart.</param>
    /// <returns>The calculated next <see cref="DateTime"/> for the restart, or <see cref="DateTime.MinValue"/> when the schedule is disabled.</returns>
    DateTime RetrieveNextRestartDate(int intervalDays, int restartClockTime);
}

