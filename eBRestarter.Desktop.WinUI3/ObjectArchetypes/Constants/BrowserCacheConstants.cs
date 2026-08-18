using eBRestarter.Desktop.WinUI3.ObjectArchetypes.DTOs.UIOptionDTO;
using System.Collections.ObjectModel;

namespace eBRestarter.Desktop.WinUI3.ObjectArchetypes.Constants;

/// <summary>
/// Provides preset configuration options for automated browser cache deletion schedules.
/// </summary>
public static class BrowserCacheConstants
{
    /// <summary>
    /// Gets the read-only collection of selectable browser cache deletion interval options.
    /// </summary>
    public static readonly ReadOnlyCollection<BrowserCacheDeleteOption> Options = new(
    [
            new("Cache und Cookies nicht löschen", 0),
            new("Cache und Cookies jeden Tag löschen", 1),
            new("Cache und Cookies alle 3 Tage löschen", 3),
            new("Cache und Cookies alle 7 Tage löschen", 7),
            new("Cache und Cookies alle 14 Tage löschen", 14)
    ]);
}

