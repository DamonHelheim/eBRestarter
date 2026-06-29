namespace eBRestarter.Core.Application.Ports.Inbound.UseCases.ManageRestarterCycle;

using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models.Errors;

public interface IManageRestarterCycleUseCase
{
    event EventHandler<RestarterCycleProgress> ProgressChanged;

    Task StartAsync(ManageRestarterCycleRequest request, Func<Task> performCleanupCallback);
    void Stop();
}

