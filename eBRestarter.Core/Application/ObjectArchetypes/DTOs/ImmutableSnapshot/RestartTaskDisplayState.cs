namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.ImmutableSnapshot;

/// <summary>
/// Represents an immutable snapshot of the restarter task state displayed in the user interface.
/// </summary>
public sealed class RestartTaskDisplayState
{
    /// <summary>
    /// Gets the configured eBesucher username.
    /// </summary>
    public string Username { get; init; } = "-";

    /// <summary>
    /// Gets the identifier or name of the selected web browser.
    /// </summary>
    public string ChosenBrowser { get; init; } = string.Empty;

    /// <summary>
    /// Gets the active browser execution duration in seconds per cycle.
    /// </summary>
    public int RuntimeSeconds { get; init; }

    /// <summary>
    /// Gets the pause duration between browser execution cycles in seconds.
    /// </summary>
    public int PauseSeconds { get; init; }

    /// <summary>
    /// Gets a value indicating whether browser cache and profile content deletion is active.
    /// </summary>
    public bool DeleteBrowserContentIsActive { get; init; }

    /// <summary>
    /// Gets the localized message indicating whether deletion feature is enabled or disabled.
    /// </summary>
    public string DeleteIsActivatedMessage { get; init; } = string.Empty;

    /// <summary>
    /// Gets the formatted status message for the upcoming deletion process.
    /// </summary>
    public string NextDeletionProcessMessage { get; init; } = string.Empty;

    /// <summary>
    /// Gets the formatted date and time message for the next scheduled deletion process.
    /// </summary>
    public string NextDeletionProcessDateMessage { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the browser process health monitoring routine is enabled.
    /// </summary>
    public bool CheckBrowserAliveRoutine { get; init; }
}
