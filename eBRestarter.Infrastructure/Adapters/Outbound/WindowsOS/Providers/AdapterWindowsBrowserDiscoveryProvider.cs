using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;
using Microsoft.Extensions.Logging;

namespace eBRestarter.Infrastructure.Adapters.Outbound.WindowsOS.Providers;

/// <summary>
/// Adapter: Driven Adapter (Outbound Provider) for discovering installed web browsers on Windows.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter / Provider)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Baustein im äußeren Ring (Infrastructure Layer) Vorgaben aus dem Core zur Systemsuche nach installierten Browsern via Registry und Dateisystem.<br/>
/// - <strong>Implementierter Port:</strong> <see cref="IOutboundPortBrowserDiscoveryProvider"/> (aus dem Application Core).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein vorbildlicher <strong>Outbound Adapter</strong> (Taxonomie: Provider Adapter), da sie im Infrastructure-Layer liegt, einen Outbound Port implementiert und vom Core angetrieben wird, um OS-spezifische Browser-Erkennungen auszuführen.
/// </para>
/// </summary>
public sealed class AdapterWindowsBrowserDiscoveryProvider(IOutboundPortBrowserFactory browserFactoryPort, ILogger<AdapterWindowsBrowserDiscoveryProvider> logger) : IOutboundPortBrowserDiscoveryProvider
{
    private readonly IOutboundPortBrowserFactory _browserFactory = browserFactoryPort;
    private readonly ILogger<AdapterWindowsBrowserDiscoveryProvider> _logger = logger;

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
                IOutboundPortBrowser browserLogic = _browserFactory.Create(type);

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




