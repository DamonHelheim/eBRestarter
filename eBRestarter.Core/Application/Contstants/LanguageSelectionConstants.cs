using eBRestarter.Core.Domain.Models.Records;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace eBRestarter.Core.Application.Contstants
{
    public class LanguageSelectionConstants
    {
        public static readonly ReadOnlyCollection<LanguageOption> Options = new(
        [
            new("Deutsch", 0),
            new("Englisch", 1)
        ]);
    }
}
