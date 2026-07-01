namespace eBRestarter.Core.Application.Ports.Inbound.Services;

public interface IComputerRestartService
{
    // Event triggered when the scheduler postpones or reschedules the computer restart timeline
    event EventHandler<DateTime?>? OnNextRestartDateChanged;

    // Starts the background monitoring loop process
    void StartScheduler();

    // Stops the background monitoring process
    Task StopSchedulerAsync();
}