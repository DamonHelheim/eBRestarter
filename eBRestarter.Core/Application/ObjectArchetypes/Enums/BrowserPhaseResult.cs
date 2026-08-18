namespace eBRestarter.Core.Application.ObjectArchetypes.Enums;

/// <summary>
/// Represents the execution outcome of a browser cycle phase.
/// </summary>
public enum BrowserPhaseResult
{
    /// <summary>
    /// The browser runtime phase completed normally for its designated duration.
    /// </summary>
    Completed,

    /// <summary>
    /// The browser phase terminated early because the browser process was closed or died.
    /// </summary>
    BrowserClosed,

    /// <summary>
    /// The browser phase was interrupted because a scheduled midnight cleanup operation is due.
    /// </summary>
    CleanupDue
}
