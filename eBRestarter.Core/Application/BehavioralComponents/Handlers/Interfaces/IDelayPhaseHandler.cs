using eBRestarter.Core.Application.ObjectArchetypes.Enums;

namespace eBRestarter.Core.Application.BehavioralComponents.Handlers.Interfaces;

public interface IDelayPhaseHandler
{
    Task ExecuteAsync(RestartTaskState state, int totalSeconds, Action<RestartTaskState, int, string?> reportProgress, CancellationToken token);
}
