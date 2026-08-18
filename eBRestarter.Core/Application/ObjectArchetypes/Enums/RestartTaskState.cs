namespace eBRestarter.Core.Application.ObjectArchetypes.Enums;

/// <summary>
/// Specifies the overall lifecycle state of the restarter task execution.
/// </summary>
public enum RestartTaskState
{
    /// <summary>
    /// The restarter cycle is idle and not executing.
    /// </summary>
    Idle,

    /// <summary>
    /// The restarter cycle is in its initial startup delay phase before launching the browser.
    /// </summary>
    InitialDelay,

    /// <summary>
    /// The restarter cycle is actively running and executing the browser runtime phase.
    /// </summary>
    Running,

    /// <summary>
    /// The restarter cycle is in the cooldown pause phase between browser execution runs.
    /// </summary>
    Cooldown
}