namespace eBRestarter.Desktop.WinUI3.ObjectArchetypes.DTOs.SignalDTO.Messages;

/// <summary>
/// Signal message published when the configured user account username is changed.
/// </summary>
/// <param name="NewUsername">The updated username value.</param>
public sealed record UsernameChangedMessage(string NewUsername);
