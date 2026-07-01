using eBRestarter.Core.Application.Enums;

namespace eBRestarter.Core.Application.Ports.Inbound.Handlers;

public interface IDelayPhaseHandler
{
    Task ExecuteAsync(RestartTaskState state, int totalSeconds, Action<RestartTaskState, int, string?> reportProgress, CancellationToken token);
}
