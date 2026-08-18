namespace eBRestarter.Desktop.WinUI3.ObjectArchetypes.DTOs.UIOptionDTO;

/// <summary>
/// Represents a selectable UI dropdown option for scheduled computer restarts.
/// </summary>
/// <param name="DisplayText">The localized display label shown in the UI selection control.</param>
/// <param name="Days">The restart interval in days (0 indicates automated restart disabled).</param>
public sealed record ComputerRestartOption(string DisplayText, int Days);
