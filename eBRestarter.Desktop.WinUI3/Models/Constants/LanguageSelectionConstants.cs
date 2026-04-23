using eBRestarter.Desktop.WinUI3.Models;
using System.Collections.ObjectModel;

namespace eBRestarter.Desktop.WinUI3.Models.Constants;

    public static class LanguageSelectionConstants
    {
        public static readonly ReadOnlyCollection<LanguageOption> Options = new(
        [
            new("Deutsch", 0),
            new("Englisch", 1)
        ]);
    }
