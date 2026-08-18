using eBRestarter.Desktop.WinUI3.ObjectArchetypes.DTOs.UIOptionDTO;
using System.Collections.ObjectModel;

namespace eBRestarter.Desktop.WinUI3.ObjectArchetypes.Constants;

/// <summary>
/// Provides preset configuration options for application UI language selection.
/// </summary>
public static class LanguageSelectionConstants
{
    /// <summary>
    /// Gets the read-only collection of supported UI language options.
    /// </summary>
    public static readonly ReadOnlyCollection<LanguageOption> Options = new(
    [
        new("Deutsch", 0),
        new("Englisch", 1)
    ]);
}
