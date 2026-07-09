using eBRestarter.Desktop.WinUI3.ObjectArchetypes.DTOs.UIOptionDTO;
using System.Collections.ObjectModel;

namespace eBRestarter.Desktop.WinUI3.ObjectArchetypes.Constants;

public static class LanguageSelectionConstants
{
    public static readonly ReadOnlyCollection<LanguageOption> Options = new(
    [
        new("Deutsch", 0),
        new("Englisch", 1)
    ]);
}
