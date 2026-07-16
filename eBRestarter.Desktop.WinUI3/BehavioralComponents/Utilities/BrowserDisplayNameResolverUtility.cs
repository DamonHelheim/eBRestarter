using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Utilities.Interfaces;

namespace eBRestarter.Desktop.WinUI3.BehavioralComponents.Utilities;

public sealed class BrowserDisplayNameResolverUtility : IBrowserDisplayNameResolverUtility
{

    public BrowserType ResolveBrowserTypeFromDisplayName(string displayName, string defaultDisplayText)
    {

        if (string.IsNullOrWhiteSpace(displayName) || displayName == defaultDisplayText || displayName == "Nicht gewählt")
        {
            return BrowserType.Chrome;
        }

        if (System.Enum.TryParse(displayName, true, out BrowserType type))
        {
            return type;
        }

        return BrowserType.Chrome;

    }
}






