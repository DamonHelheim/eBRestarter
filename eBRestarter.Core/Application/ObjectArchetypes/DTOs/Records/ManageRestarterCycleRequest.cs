using eBRestarter.Core.Application.ObjectArchetypes.Enums;

namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

public sealed record ManageRestarterCycleRequest(
    BrowserType BrowserType,
    string Username,
    int RuntimeSeconds,
    int PauseSeconds,
    bool CheckBrowserAliveRoutine
);


