namespace eBRestarter.Desktop.WinUI3.ObjectArchetypes.DTOs.SignalDTO.Messages;

/// <summary>
/// Signal message containing status information regarding the next deletion process.
/// </summary>
/// <param name="Message">The status message payload detailing the upcoming deletion process.</param>
public sealed record class NextDeletionProcessMessage(string Message);

