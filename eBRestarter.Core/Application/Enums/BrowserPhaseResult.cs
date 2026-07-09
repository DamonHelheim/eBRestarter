namespace eBRestarter.Core.Application.Enums;

public enum BrowserPhaseResult
{
    Completed,     // The runtime has completed normally
    BrowserClosed, // The CheckBrowserAliveRoutine triggered (browser is dead)
    CleanupDue     // 12:00 AM (midnight) was reached, clean up immediately!
}
