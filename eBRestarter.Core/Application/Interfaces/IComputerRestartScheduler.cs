namespace eBRestarter.Core.Application.Interfaces;

public interface IComputerRestartScheduler
{
    // Event, das ausgelöst wird, wenn der Scheduler den Rechner-Neustart verschiebt
    event EventHandler<DateTime?>? OnNextRestartDateChanged;

    // Startet die Überwachung im Hintergrund
    void StartScheduler();

    // Stoppt die Überwachung
    Task StopSchedulerAsync();


}
