using eBRestarter.Core.Application.Enums;

namespace eBRestarter.Desktop.WinUI3.Utilities;

/// <summary>
/// Port: Resolves the display name of the browser (localized) into a BrowserType.
/// </summary>
public interface IBrowserDisplayNameResolverUtility
{
    /// <summary>
    /// Determines the BrowserType from the display name (e.g., ComboBox text).
    /// </summary>
    /// <param name="displayName">The display name or empty/null.</param>
    /// <param name="defaultDisplayText">The localized text for "None selected" (for matching purposes).</param>
    /// <returns>The resolved BrowserType, falling back to Chrome if unknown or empty.</returns>
    BrowserType ResolveBrowserTypeFromDisplayName(string displayName, string defaultDisplayText);
}

