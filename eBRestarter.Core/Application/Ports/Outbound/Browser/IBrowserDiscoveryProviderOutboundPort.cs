using eBRestarter.Core.Application.Models;

namespace eBRestarter.Core.Application.Ports.Outbound.Browser;

public interface IBrowserDiscoveryProviderOutboundPort
{
    /// <summary>
    /// Performs a read-only system search and returns browser metadata to the Core.
    /// Note: The original suffix was redundant (ServiceProvider). The name has been cleaned up to be a pure Provider.
    /// </summary>
    Task<IEnumerable<BrowserInfo>> FindInstalledBrowsersAsync();
}

