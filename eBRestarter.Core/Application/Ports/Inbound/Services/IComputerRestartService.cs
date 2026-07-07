namespace eBRestarter.Core.Application.Ports.Inbound.Services;

/// <summary>
/// Port: Manages the background computer restart schedule and timeline notifications for the presentation layer.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): INBOUND PORT (System-Trigger / Application Service Port)</strong><br/>
/// - <strong>Aufrufer (Consumer):</strong> Liegt AUßERHALB des Application Cores (<see cref="eBRestarter.Desktop.WinUI3.App"/> und ViewModels im Presentation Layer).<br/>
/// - <strong>Implementierung (Implementer):</strong> Liegt INNERHALB des Application Cores (<see cref="eBRestarter.Core.Application.Services.ComputerRestartService"/>).<br/>
/// - <strong>Begründung:</strong> Dient als Eingangstür in den Anwendungskern für die UI und den Lifecycle-Host, um den zyklischen Neustart-Scheduler zu starten, zu stoppen und auf Terminäderungen zu lauschen.<br/>
/// - <em>Architektur-Hinweis:</em> Suffix sollte gemäß Leitfaden idealerweise auf <c>Port</c> (z. B. <c>IComputerRestartServicePort</c>) enden.
/// </para>
/// </summary>
public interface IComputerRestartService
{
    // Event triggered when the scheduler postpones or reschedules the computer restart timeline
    event EventHandler<DateTime?>? OnNextRestartDateChanged;

    // Starts the background monitoring loop process
    void StartScheduler();

    // Stops the background monitoring process
    Task StopSchedulerAsync();
}