using eBRestarter.Core.Domain.Models;

namespace eBRestarter.Core.Application.Interfaces.Browser;

public interface IBrowserService
{
    Task<IEnumerable<BrowserInfo>> GetInstalledBrowsersAsync();
    // Eventuell Methoden für Installation, Download etc.
}
