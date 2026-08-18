namespace eBRestarter.Desktop.WinUI3.ObjectArchetypes.DTOs.SignalDTO.Messages;

/// <summary>
/// Signal message published to trigger or update the activation status of browser content deletion.
/// </summary>
/// <param name="ActivateMessage">The activation status message payload.</param>
public sealed record DeleteBrowserContentActivateMessage(string ActivateMessage);
