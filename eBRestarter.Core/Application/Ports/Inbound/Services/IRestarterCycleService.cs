using eBRestarter.Core.Application.Models.Records;

namespace eBRestarter.Core.Application.Ports.Inbound.Services;

public interface IRestarterCycleService
{
    event EventHandler<RestarterCycleProgress> ProgressChanged;

    Task StartAsync(ManageRestarterCycleRequest request, Func<Task> performCleanupCallback);
    void Stop();
}

