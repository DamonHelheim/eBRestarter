using eBRestarter.Desktop.WinUI3.Models;
using System.Collections.ObjectModel;

namespace eBRestarter.Desktop.WinUI3.Models.Constants;

    public static class BrowserCacheConstants
    {
        // Statische ReadOnly Liste für die Auswahl
        public static readonly ReadOnlyCollection<BrowserCacheDeleteOption> Options = new(
        [
            new("Cache und Cookies nicht löschen", 0),       // Index 0
            new("Cache und Cookies jeden Tag löschen", 1),   // Index 1
            new("Cache und Cookies alle 3 Tage löschen", 3),
            new("Cache und Cookies alle 7 Tage löschen", 7),
            new("Cache und Cookies alle 14 Tage löschen", 14)
        ]);
    }

