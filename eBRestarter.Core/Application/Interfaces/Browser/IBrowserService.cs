using eBRestarter.Core.Application.Models;

namespace eBRestarter.Core.Application.Interfaces.Browser;

public interface IBrowserService
{
    Task<IEnumerable<BrowserInfo>> FindInstalledBrowsersAsync();
    // Eventuell Methoden für Installation, Download etc.
}
