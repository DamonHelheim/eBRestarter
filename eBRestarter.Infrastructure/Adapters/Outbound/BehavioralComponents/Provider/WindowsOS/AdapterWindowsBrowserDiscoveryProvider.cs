using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.ObjectArchetypes.Models;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Provider.WindowsOS;

/// <summary>
/// Adapter: Driven Adapter (Outbound Provider) for discovering installed web browsers on Windows.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter / Provider)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Baustein im äußeren Ring (Infrastructure Layer) Vorgaben aus dem Core zur Systemsuche nach installierten Browsern via Registry und Dateisystem.<br/>
/// - <strong>Implementierter Port:</strong> <see cref="IOutboundPortBrowserDiscoveryProvider"/> (aus dem Application Core).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein vorbildlicher <strong>Outbound Adapter</strong> (Taxonomie: Provider Adapter), da sie im Infrastructure-Layer liegt, einen Outbound Port implementiert und vom Core angetrieben wird, um OS-spezifische Browser-Erkennungen auszuführen.
/// </para>
/// </summary>
public sealed class AdapterWindowsBrowserDiscoveryProvider : IOutboundPortBrowserDiscoveryProvider
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitive Typen & Strings ──
    private const string BrowserTypeNotSupportedLogMessage = "Browser type {Type} is currently not supported by the factory implementation.";
    private const string ErrorEvaluatingBrowserDetailsLogMessage = "An error occurred while evaluating details for browser type {Type}";
    private const string NotInstalledStatusText = "Not installed";

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    // ── Block 1: Injizierte Abhängigkeiten (Dependencies) ──
    private readonly IOutboundPortBrowserFactory _browserFactory;
    private readonly ILogger<AdapterWindowsBrowserDiscoveryProvider> _logger;


    // ═══════════════════════════════════════════════════════
    //  6. Constructors
    // ═══════════════════════════════════════════════════════
    public AdapterWindowsBrowserDiscoveryProvider(
        IOutboundPortBrowserFactory browserFactory,
        ILogger<AdapterWindowsBrowserDiscoveryProvider> logger)
    {
        ArgumentNullException.ThrowIfNull(browserFactory);
        ArgumentNullException.ThrowIfNull(logger);

        _browserFactory = browserFactory;
        _logger = logger;
    }


    // ═══════════════════════════════════════════════════════
    //  8. Methods (public → private)
    // ═══════════════════════════════════════════════════════
    public Task<IEnumerable<BrowserInfo>> FindInstalledBrowsersAsync()
    {
        var browsers = new List<BrowserInfo>();

        foreach (BrowserType type in Enum.GetValues<BrowserType>())
        {
            try
            {
                IOutboundPortBrowser browserLogic = _browserFactory.Create(type);

                bool isInstalled = browserLogic.IsInstalled;
                string version = isInstalled ? browserLogic.BrowserVersion : NotInstalledStatusText;
                string displayName = browserLogic.DisplayName;
                string iconPath = browserLogic.IconPath;
                string downloadUrl = browserLogic.DownloadUrl;

                var browserInfo = new BrowserInfo
                {
                    Type = type,
                    Name = displayName,
                    IsInstalled = isInstalled,
                    Version = version,
                    IconPath = iconPath,
                    DownloadUrl = downloadUrl
                };

                browsers.Add(browserInfo);
            }
            catch (NotSupportedException exception)
            {
                _logger.LogWarning(exception, BrowserTypeNotSupportedLogMessage, type);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, ErrorEvaluatingBrowserDetailsLogMessage, type);
            }
        }

        return Task.FromResult<IEnumerable<BrowserInfo>>(browsers);
    }
}
