namespace eBRestarter.Desktop.WinUI3.ObjectArchetypes.DTOs.SignalDTO.Messages;

/// <summary>
/// Signal message published when the selected target browser is changed.
/// </summary>
/// <param name="BrowserName">The display name of the newly selected browser.</param>
public sealed record BrowserChangedMessage(string BrowserName);
