using eBRestarter.Core.Application.Enums;

namespace eBRestarter.Core.Application.Handlers.ManageRestarterCycle.Strategies;

public interface IDelayPhaseStrategy
{
    Task ExecuteAsync(RestartTaskState state, int totalSeconds, Action<RestartTaskState, int, string?> reportProgress, CancellationToken token);
}
