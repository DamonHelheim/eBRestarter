using eBRestarter.Core.Domain.Models.Records;
using System.Collections.ObjectModel;

namespace eBRestarter.Core.Application.Contstants;

    public static class LanguageSelectionConstants
    {
        public static readonly ReadOnlyCollection<LanguageOption> Options = new(
        [
            new("Deutsch", 0),
            new("Englisch", 1)
        ]);
    }
