using eBRestarter.Core.Application.Enums;

namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

public record struct RestarterCycleProgress(
    RestartTaskState State,
    int SecondsRemaining,
    string StatusMessage
);

