using eBRestarter.Core.Application.ObjectArchetypes.Enums;

namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

/// <summary>
/// Request parameters for starting, stopping, or updating the restarter execution cycle.
/// </summary>
/// <param name="BrowserType">The target web browser application type.</param>
/// <param name="Username">The eBesucher account username.</param>
/// <param name="RuntimeSeconds">The active browser execution duration in seconds.</param>
/// <param name="PauseSeconds">The pause duration between execution cycles in seconds.</param>
/// <param name="CheckBrowserAliveRoutine">Indicates whether process health monitoring is enabled.</param>
public sealed record ManageRestarterCycleRequest(
    BrowserType BrowserType,
    string Username,
    int RuntimeSeconds,
    int PauseSeconds,
    bool CheckBrowserAliveRoutine
);


