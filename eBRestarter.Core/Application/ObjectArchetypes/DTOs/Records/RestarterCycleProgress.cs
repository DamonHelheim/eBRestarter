using eBRestarter.Core.Application.ObjectArchetypes.Enums;

namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

/// <summary>
/// Represents real-time progress information emitted during restarter cycle execution.
/// </summary>
/// <param name="State">The current execution state of the restarter task.</param>
/// <param name="SecondsRemaining">The remaining time in seconds for the current phase.</param>
/// <param name="StatusMessage">The localized status message describing the current phase.</param>
public record struct RestarterCycleProgress(
    RestartTaskState State,
    int SecondsRemaining,
    string StatusMessage
);

