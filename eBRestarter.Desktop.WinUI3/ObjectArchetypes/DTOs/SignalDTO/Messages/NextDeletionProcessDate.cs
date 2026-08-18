namespace eBRestarter.Desktop.WinUI3.ObjectArchetypes.DTOs.SignalDTO.Messages;

/// <summary>
/// Signal message containing formatted schedule information for the next browser deletion process.
/// </summary>
/// <param name="NextDeletionProcessDateMessage">The formatted date string representing the next deletion schedule.</param>
public sealed record class NextDeletionProcessDate(string NextDeletionProcessDateMessage);
