using System;
using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Utilities.Interfaces;

namespace eBRestarter.Desktop.WinUI3.BehavioralComponents.Utilities;

/// <summary>
/// Utility implementation for resolving localized browser display names into domain <see cref="BrowserType"/> enums.
/// </summary>
public sealed class BrowserDisplayNameResolverUtility : IBrowserDisplayNameResolverUtility
{
    /// <inheritdoc />
    public BrowserType ResolveBrowserTypeFromDisplayName(string displayName, string defaultDisplayText)
    {
        if (string.IsNullOrWhiteSpace(displayName) ||
            string.Equals(displayName, defaultDisplayText, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(displayName, "Nicht gewählt", StringComparison.OrdinalIgnoreCase))
        {
            return BrowserType.Chrome;
        }

        return Enum.TryParse(displayName, true, out BrowserType type) ? type : BrowserType.Chrome;
    }
}
