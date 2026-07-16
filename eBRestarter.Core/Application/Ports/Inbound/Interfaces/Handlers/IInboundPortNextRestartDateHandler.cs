namespace eBRestarter.Core.Application.Ports.Inbound.Interfaces.Handlers;

/// <summary>
/// Application facade for computing the next scheduled computer restart (delegates to domain calculation).
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): INBOUND PORT / USE CASE INTERFACE</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Desktop.WinUI3.ViewModels.ViewModelOptionsGeneral"/> im Presentation Layer via MVVM).<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt INNERHALB des Application Cores (<see cref="BehavioralComponents.Handlers.NextRestartDateHandler"/>).<br/>
/// - <strong>Begründung:</strong> Dient als Eingangstür in den Anwendungskern, um für die Benutzeroberfläche den nächsten geplanten Neustartzeitpunkt zu berechnen und anzuzeigen.
/// </para>
/// </summary>
public interface IInboundPortNextRestartDateHandler
{
    /// <summary>
    /// Computes the next restart instant from interval (days) and clock hour (0–23).
    /// </summary>
    /// <returns><see cref="DateTime.MinValue"/> when the schedule is disabled.</returns>
    DateTime RetrieveNextRestartDate(int intervalDays, int restartClockTime);
}

