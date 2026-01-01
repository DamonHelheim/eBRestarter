using eBRestarter.Core.Domain.Models.Records;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace eBRestarter.Core.Application.Contstants
{
    public class ComputerRestartConstants
    {
        // Statische ReadOnly Liste für die Auswahl
        public static readonly ReadOnlyCollection<ComputerRestartOption> Options = new(
        [
            new("Computer nicht neustarten", 0),
            new("Computer jeden Tag neustarten", 1),
            new("Computer alle 3 Tage neustarten", 3),
            new("Computer alle 7 Tage neustarten", 7),
            new("Computer alle 14 Tage neustarten", 14)
        ]);
    }
}
