namespace eBRestarter.Core.Application.UseCases.ManageRestarterCycle;

public interface IManageRestarterCycleUseCase
{
    event EventHandler<RestarterCycleProgress> ProgressChanged;

    Task StartAsync(ManageRestarterCycleRequest request, Func<Task> performCleanupCallback);
    void Stop();
}
