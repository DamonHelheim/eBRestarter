using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Ports.Outbound.Browser;
using eBRestarter.Core.Application.Models;
using Microsoft.Extensions.Logging;

namespace eBRestarter.Infrastructure.Adapters.WindowsOS;

public sealed class WindowsBrowserDiscoveryProviderAdapter(IBrowserFactoryPort BrowserFactory, ILogger<WindowsBrowserDiscoveryProviderAdapter> logger) : IBrowserDiscoveryPort
{
    private readonly IBrowserFactoryPort _browserFactory = BrowserFactory ?? throw new ArgumentNullException(nameof(BrowserFactory));
    private readonly ILogger<WindowsBrowserDiscoveryProviderAdapter> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<IEnumerable<BrowserInfo>> FindInstalledBrowsersAsync()
    {
        var browsers = new List<BrowserInfo>();

        // We iterate dynamically over all elements in the BrowserType enum.
        // This keeps the architecture future-proof: Adding a new browser to the enum and the Factory automatically integrates it here.
        foreach (BrowserType type in Enum.GetValues<BrowserType>())
        {
            try
            {
                // 1. Leverage the Factory to resolve the target discrete adapter logic class for this specific browser type
                // (e.g., ChromeBrowser, FirefoxBrowser, etc.)
                IBrowserPort browserLogic = _browserFactory.Create(type);

                // 2. Query target environment installation states and metrics
                // The underlying BrowserBase implementations encapsulate the hardware-specific registry and local path inspection logic
                bool isInstalled = browserLogic.IsInstalled;
                string version = isInstalled ? browserLogic.BrowserVersion : "Not installed";
                string displayName = browserLogic.DisplayName;
                string iconPath = browserLogic.IconPath;
                string downloadUrl = browserLogic.DownloadUrl;

                // 3. Populate and construct the BrowserInfo data transfer object (Mapping)
                // We map the low-level logic results directly onto a decoupled object tailored for user interface rendering
                var browserInfo = new BrowserInfo
                {
                    Type = type,
                    Name = displayName,
                    IsInstalled = isInstalled,
                    Version = version,
                    IconPath = iconPath,
                    DownloadUrl = downloadUrl,
                };

                browsers.Add(browserInfo);
            }
            catch (NotSupportedException ex)
            {
                // Triggered if a browser exists in the Enum definitions but hasn't been implemented within the target Factory instance yet.
                _logger.LogWarning(ex, "Browser type {Type} is currently not supported by the factory implementation.", type);
            }
            catch (Exception ex)
            {
                // Localized processing failures belonging to an isolated browser type should not block the collection of remaining engines
                _logger.LogError(ex, "An error occurred while evaluating details for browser type {Type}", type);
            }
        }

        // Since registry discovery operations run synchronously, we wrap the result array safely inside a Task completion context
        return await Task.FromResult(browsers);
    }
}




