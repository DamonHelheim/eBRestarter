namespace eBRestarter.Core.Application.UseCases.ManageRestarterCycle;

public record ManageRestarterCycleRequest(
    string BrowserDisplayName,
    string Username,
    int RuntimeSeconds,
    int PauseSeconds,
    bool CheckBrowserAliveRoutine
);
