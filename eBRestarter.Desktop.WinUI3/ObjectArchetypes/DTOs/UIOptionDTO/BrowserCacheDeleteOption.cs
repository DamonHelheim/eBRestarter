namespace eBRestarter.Desktop.WinUI3.ObjectArchetypes.DTOs.UIOptionDTO;

/// <summary>
/// Represents a selectable UI dropdown option for scheduled browser cache deletion.
/// </summary>
/// <param name="DisplayText">The localized display label shown in the UI selection control.</param>
/// <param name="Days">The deletion interval in days (0 indicates deletion disabled).</param>
public sealed record BrowserCacheDeleteOption(string DisplayText, int Days);
