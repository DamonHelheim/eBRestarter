namespace eBRestarter.Core.Application.Enums;

public enum RestartTaskState
{
    Idle,           // Nothing is running
    InitialDelay,   // "StartRestartTimer" (The first 5 seconds)
    Running,        // "mainTimer" (Browser is open)
    Cooldown        // "BrowserStartsInTimer" (Pause / Restart in...)
}