using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Interfaces.Browser;

namespace eBRestarter.Infrastructure.Services;

public class BrowserDisplayNameResolverHandler : IBrowserDisplayNameResolverUseCase
{

    public BrowserType ResolveBrowserTypeFromDisplayName(string displayName, string defaultDisplayText)
    {

        if (string.IsNullOrWhiteSpace(displayName) || displayName == defaultDisplayText || displayName == "Nicht gewÃ¤hlt")
        {
            return BrowserType.Chrome;
        }

        if (Enum.TryParse(displayName, true, out BrowserType type))
        {
            return type;
        }

        return BrowserType.Chrome;

    }
}

