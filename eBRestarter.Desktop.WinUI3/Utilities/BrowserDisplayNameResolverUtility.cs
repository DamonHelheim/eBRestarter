using eBRestarter.Core.Application.Enums;

namespace eBRestarter.Desktop.WinUI3.Utilities;

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






