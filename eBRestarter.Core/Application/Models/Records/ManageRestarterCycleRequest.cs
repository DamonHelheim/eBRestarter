namespace eBRestarter.Core.Application.Models.Records;

public sealed record ManageRestarterCycleRequest(
    eBRestarter.Core.Application.Enums.BrowserType BrowserType,
    string Username,
    int RuntimeSeconds,
    int PauseSeconds,
    bool CheckBrowserAliveRoutine
);


