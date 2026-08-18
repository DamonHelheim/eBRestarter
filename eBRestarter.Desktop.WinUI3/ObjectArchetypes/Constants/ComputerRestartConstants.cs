using eBRestarter.Desktop.WinUI3.ObjectArchetypes.DTOs.UIOptionDTO;
using System.Collections.ObjectModel;

namespace eBRestarter.Desktop.WinUI3.ObjectArchetypes.Constants
{
    /// <summary>
    /// Provides preset configuration options for scheduled system restart intervals.
    /// </summary>
    public static class ComputerRestartConstants
    {
        /// <summary>
        /// Gets the read-only collection of selectable computer restart interval options.
        /// </summary>
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
