using eBRestarter.Core.Domain.Enums;

namespace eBRestarter.Core.Application.UseCases.ManageRestarterCycle;

public record struct RestarterCycleProgress(
    RestartTaskState State,
    int SecondsRemaining,
    string StatusMessage
);
