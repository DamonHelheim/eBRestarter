using eBRestarter.Core.Application.Models.Records;

namespace eBRestarter.Core.Application.Ports.Inbound.UseCases.ManageRestarterCycle;

public interface IManageRestarterCycleUseCase
{
    event EventHandler<RestarterCycleProgress> ProgressChanged;

    Task StartAsync(ManageRestarterCycleRequest request, Func<Task> performCleanupCallback);
    void Stop();
}

