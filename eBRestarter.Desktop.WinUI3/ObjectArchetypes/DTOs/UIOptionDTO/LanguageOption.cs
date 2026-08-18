namespace eBRestarter.Desktop.WinUI3.ObjectArchetypes.DTOs.UIOptionDTO;

/// <summary>
/// Represents a selectable UI dropdown option for application language selection.
/// </summary>
/// <param name="DisplayText">The localized name of the language shown in the UI selection control.</param>
/// <param name="Index">The numerical identifier associated with the language option.</param>
public sealed record LanguageOption(string DisplayText, int Index);
